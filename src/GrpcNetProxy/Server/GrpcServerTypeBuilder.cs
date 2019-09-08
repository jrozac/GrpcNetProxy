using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace GrpcNetProxy.Server
{

    /// <summary>
    /// Service type builder for services build from proto files
    /// </summary>
    public class GrpcServerTypeBuilder
    {

        /// <summary>
        /// Service interface (basic properties for service)
        /// </summary>
        public interface IService
        {
            IServiceProvider ServiceProvider { get; set; }
            Dictionary<string, Delegate> Invokers { get; set; }
            string HostCfgName { get; set; }
        }

        /// <summary>
        /// Build type for grpc service
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="cfgName"></param>
        /// <returns></returns>
        internal static Type Build(Type serviceType, string cfgName = "Default")
        {

            // type meta-data
            var assemblyName = Guid.NewGuid().ToString();
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
            var typeBuilder = moduleBuilder.DefineType(serviceType.Name + "Server", TypeAttributes.Public, serviceType);

            // add interface 
            ImplementServiceInterface(typeBuilder);

            // add constructor
            AddConstructor(typeBuilder, cfgName);

            // add service methods
            var methods = GetGrpcMethodsForServiceType(serviceType);
            methods.Keys.ToList().ForEach(m => AddMethod(typeBuilder, methods.First(m2 => m2.Key == m)));

            // build type and return
            var svcType = typeBuilder.CreateTypeInfo();
            return svcType;
        }

        /// <summary>
        /// Add property to type builder
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        private static void AddProperty(TypeBuilder typeBuilder, Type type, string name)
        {
            // define property
            var fieldBuilder = typeBuilder.DefineField("_" + name, type, FieldAttributes.Private);
            var propertyBuilder = typeBuilder
                .DefineProperty(name, PropertyAttributes.None, CallingConventions.HasThis, type, null);
            var getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

            // getter
            var getter = typeBuilder.DefineMethod("get_" + name, getSetAttr, type, Type.EmptyTypes);
            var getIL = getter.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            // setter 
            var setter = typeBuilder.DefineMethod("set_" + name, getSetAttr, null, new[] { type });
            var setIL = setter.GetILGenerator();
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);

            // add getter and setter methods
            propertyBuilder.SetGetMethod(getter);
            propertyBuilder.SetSetMethod(setter);
        }

        /// <summary>
        /// Implement service inteface
        /// </summary>
        /// <param name="myTypeBuilder"></param>
        private static void ImplementServiceInterface(TypeBuilder myTypeBuilder)
        {

            // add interface 
            myTypeBuilder.AddInterfaceImplementation(typeof(IService));

            // add required properties
            AddProperty(myTypeBuilder, typeof(IServiceProvider), "ServiceProvider");
            AddProperty(myTypeBuilder, typeof(Dictionary<string, Delegate>), "Invokers");
            AddProperty(myTypeBuilder, typeof(string), "HostCfgName");
        }

        /// <summary>
        /// Add method to type
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="method"></param>
        private static void AddMethod(TypeBuilder typeBuilder, KeyValuePair<string, MethodInfo> method)
        {

            // service type 
            var svcType = method.Value.DeclaringType;

            // method setup parameters
            var args = method.Value.GetParameters();
            var methodBuilder = typeBuilder.DefineMethod(method.Value.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                method.Value.ReturnType,
                (from arg in args select arg.ParameterType).ToArray()
            );
            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); //load this
            il.Emit(OpCodes.Ldarg_1); //load request
            il.Emit(OpCodes.Ldarg_2); // server context
            il.Emit(OpCodes.Ldstr, method.Key); // method name

            // set method to run
            var callMethodTemplate = typeof(GrpcServerTypeBuilder).GetMethod(nameof(GrpcServerTypeBuilder.Execute), BindingFlags.Static | BindingFlags.Public);
            var retType = method.Value.ReturnType.GenericTypeArguments.First();
            var reqType = method.Value.GetParameters().First().ParameterType;
            var callMethod = callMethodTemplate.MakeGenericMethod(reqType, retType, svcType);
            il.Emit(OpCodes.Call, callMethod);

            // set return 
            il.Emit(OpCodes.Ret);

            // set method to builder
            typeBuilder.DefineMethodOverride(methodBuilder, method.Value);
        }

        /// <summary>
        /// Add constructor to type builder
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="cfgName"></param>
        private static void AddConstructor(TypeBuilder typeBuilder, string cfgName)
        {

            // define constructor builder
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] { typeof(IServiceProvider) }
            );

            // get method to be executed for constructor
            var constructorMethod = typeof(GrpcServerTypeBuilder)
                .GetMethod(nameof(GrpcServerTypeBuilder.Constructor), BindingFlags.Static | BindingFlags.Public);

            // set constructor
            var il = ctorBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // load this
            il.Emit(OpCodes.Ldarg_1); // service provider
            il.Emit(OpCodes.Ldstr, cfgName); // cfg name
            il.Emit(OpCodes.Call, constructorMethod);
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Get methods which are defined to be handlers for GRPC
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private static Dictionary<string,MethodInfo> GetGrpcMethodsForServiceType(Type serviceType)
        {
            int i = 0;
            return serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetParameters().Any() && m.GetParameters().Last().ParameterType == typeof(ServerCallContext))
                .ToDictionary(m => $"{m.Name}_{i++}", m => m);
        }

        /// <summary>
        /// Constructor procedure
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="cfgName"></param>
        public static void Constructor(object _this, IServiceProvider serviceProvider, string cfgName)
        {
            var host = serviceProvider.GetServices<GrpcHost>().First(h => h.Name == cfgName);
            var cfg = host.Configuration;

            // set service type 
            var serviceType = _this.GetType().BaseType;

            // save service provider
            IService dthis = (IService)_this;
            dthis.ServiceProvider = serviceProvider;
            dthis.HostCfgName = cfgName;

            // create delegate invokers
            var methods = GetGrpcMethodsForServiceType(serviceType);
            var invokers = methods.ToDictionary(m => m.Key, m => {

                var prms = m.Value.GetParameters();
                var dlgtType = typeof(Func<,,,>)
                    .MakeGenericType(serviceType, prms[0].ParameterType, prms[1].ParameterType, m.Value.ReturnType);
                var dlgt = Delegate.CreateDelegate(dlgtType, m.Value);
                return dlgt;
            });
            dthis.Invokers = invokers;

        }

        /// <summary>
        /// Execute service method 
        /// </summary>
        /// <typeparam name="TReq"></typeparam>
        /// <typeparam name="TRet"></typeparam>
        /// <param name="@this"></param>
        /// <param name="req"></param>
        /// <param name="ctxt"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static Task<TRet> Execute<TReq, TRet,TSvc>(object @this, TReq req, ServerCallContext ctxt, string methodName)
            where TReq : class
            where TRet : class
            where TSvc : class
        {

            // get execution parameters
            var provider = GetServiceProvider(@this);
            var hostName = GetHostCfgName(@this);
            var invoker = (Func<TSvc, TReq, ServerCallContext, Task<TRet>>) GetInvoker(@this, methodName);

            // execute request
            return RequestHandler.HandleRequest<TReq, TRet, TSvc>(provider, req, invoker, hostName, methodName, ctxt);
        }

        /// <summary>
        /// Get service provider for service
        /// </summary>
        /// <param name="@this"></param>
        /// <returns></returns>
        private static IServiceProvider GetServiceProvider(object @this)
        {
            return ((IService)@this).ServiceProvider;
        }

        /// <summary>
        /// Get host cfg name 
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        private static string GetHostCfgName(object @this)
        {
            return ((IService)@this).HostCfgName;
        }

        /// <summary>
        /// Get service method invoker
        /// </summary>
        /// <param name="@this"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private static Delegate GetInvoker(object @this, string methodName) {

            return ((IService)@this).Invokers[methodName];
        }

    }
}

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Client type builder. Used to build client class type according to interface definition
    /// </summary>
    internal static class GrpcClientTypeBuilder
    {

        /// <summary>
        /// Create grpc client type 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TypeInfo Create<TService>() where TService : class
        {

            // type meta-data
            var assemblyName = Guid.NewGuid().ToString();
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
            var serviceType = typeof(TService);
            var typeBuilder = moduleBuilder.DefineType(serviceType.Name + "Client", TypeAttributes.Public, typeof(GrpcClientBase));

            // type definition 
            typeBuilder.AddInterfaceImplementation(serviceType);
            AddConstructor(typeBuilder, serviceType);
            AddMethods(typeBuilder, serviceType);

            // create type adn return 
            return typeBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// Add constructor
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="serviceType"></param>
        private static void AddConstructor(TypeBuilder typeBuilder, Type serviceType)
        {

            // get base type constructor
            var clientBaseType = typeof(GrpcClientBase);
            var baseConstructor = clientBaseType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
            var paramsTypes = baseConstructor.GetParameters().Select(p => p.ParameterType).ToArray();

            // define constructor builder
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                paramsTypes
            );

            // set constructor
            var il = ctorBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); //load this
            Enumerable.Range(1, paramsTypes.Length).ToList().ForEach(i => il.Emit(OpCodes.Ldarg_S, i));
            var baseClassConstructor = clientBaseType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                paramsTypes, null);
            il.Emit(OpCodes.Call, baseClassConstructor); 
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Add methods for type 
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="serviceType"></param>
        private static void AddMethods(TypeBuilder typeBuilder, Type serviceType)
        {
            serviceType.GetMethods().ToList().ForEach(m => AddMethod(typeBuilder, m));
        }

        /// <summary>
        /// Add methods to type builder
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="method"></param>
        private static void AddMethod(TypeBuilder typeBuilder, MethodInfo method)
        {
        
            // get service and method names 
            var serviceName = method.DeclaringType.Name;
            var methodName = method.Name;

            // set method properties
            var args = method.GetParameters();
            var methodBuilder = typeBuilder.DefineMethod(method.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                method.ReturnType,
                (from arg in args select arg.ParameterType).ToArray()
            );
            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); //load this
            il.Emit(OpCodes.Ldarg_1); //load request
            il.Emit(OpCodes.Ldstr, serviceName); // service name
            il.Emit(OpCodes.Ldstr, methodName); // method name
            il.Emit(OpCodes.Ldarg_2); // cancellation token
            var clientBaseType = typeof(GrpcClientBase);
            var methodToCall = clientBaseType.GetMethod("CallUnaryMethodAsync", BindingFlags.Instance | BindingFlags.NonPublic);

            // set method CallUnaryMethodAsync to be called with valid arguments when interface method is called
            il.Emit(OpCodes.Call,
                methodToCall.MakeGenericMethod(new[]{method.GetParameters()[0].ParameterType, method.ReturnType.GetGenericArguments()[0]}));

            // set return 
            il.Emit(OpCodes.Ret);

            // set method to builder
            typeBuilder.DefineMethodOverride(methodBuilder, method);
        }

    }
}

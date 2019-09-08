using Grpc.Core;
using GrpcNetProxy.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace GrpcNetProxy.Server
{
    /// <summary>
    /// Grpc server builder
    /// </summary>
    internal static class GrpcServerBuilder
    {

        /// <summary>
        /// Build server grpc
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cfg"></param>
        /// <returns></returns>
        internal static Grpc.Core.Server Build(IServiceProvider provider, GrpcServerConfiguration cfg)
        {

            // get logger if needed
            var logger = (cfg.Options?.LogRequests ?? false) ? provider.GetService<ILoggerFactory>()?.CreateLogger("GrpcServerRequests") : null;

            // create services for interfaces
            var interfacesServices = CreateServiceDefinitionForInterfaces(provider, cfg);

            // implemented services
            var protoGenServices = CreateServicesDefinitionsForProtoGen(provider, cfg);

            // create grpc server 
            var server = new Grpc.Core.Server()
            {
                Ports = { { cfg.Connection.Url, cfg.Connection.Port, ServerCredentials.Insecure } },
                Services = {}
            };

            // add services to server
            interfacesServices.ForEach(s => server.Services.Add(s));
            protoGenServices.ForEach(s => server.Services.Add(s));

            // return server
            return server;

        }

        /// <summary>
        /// Create services definitions for proto generated services
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cfg"></param>
        /// <returns></returns>
        private static List<ServerServiceDefinition> CreateServicesDefinitionsForProtoGen(IServiceProvider provider,  GrpcServerConfiguration cfg)
        {
            // create definitions
            var servicesImpl = cfg.ServicesTypes.Where(t => !t.IsInterface).Select(svcType =>
            {

                // get bind method from generated code 
                var baseType = GrpcServerTypeBuilder.Build(svcType, cfg.Name);

                // var baseType = svcType.BaseType;
                var bind = (BindServiceMethodAttribute)baseType.GetCustomAttributes().First(a => a.GetType() == typeof(BindServiceMethodAttribute));
                var bindMethod = bind.BindType.GetMethod(bind.BindMethodName, new Type[] { baseType });

                // create service intstance and bind 
                var svc = Activator.CreateInstance(baseType, new object[] { provider });
                ServerServiceDefinition svcs = (ServerServiceDefinition)bindMethod.Invoke(null, new object[] { svc });
                return svcs;

            }).ToList();

            // return
            return servicesImpl;
        }

        /// <summary>
        /// Get service definition for interfaces
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cfg"></param>
        /// <returns></returns>
        private static List<ServerServiceDefinition> CreateServiceDefinitionForInterfaces(IServiceProvider provider, GrpcServerConfiguration cfg)
        {
           
            // get types to create services for 
            var servicesTypes = cfg.ServicesTypes
                .Where(t => t.IsInterface && t.GetMethods(BindingFlags.Public | BindingFlags.Instance).Any());

            // create services
            var services = servicesTypes.Select(type => {

                // create builder
                var builder = ServerServiceDefinition.CreateBuilder();

                // register methods 
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).ToList();
                methods.ForEach(m => RegisterServiceMethodToGrpc(m, provider, builder, cfg));

                // build interfaces services
                var service = builder.Build();
                return service;

            }).ToList();

            // return
            return services;
        }

        /// <summary>
        /// Register service methods to grpc
        /// </summary>
        /// <param name="method"></param>
        /// <param name="provider"></param>
        /// <param name="builder"></param>
        /// <param name="cfg"></param>
        public static void RegisterServiceMethodToGrpc(MethodInfo method, IServiceProvider provider, 
            ServerServiceDefinition.Builder builder, GrpcServerConfiguration cfg)
        {

            // get service data 
            var serviceType = method.DeclaringType;
            var serviceName = serviceType.Name;

            // get method data
            var methodName = method.Name;
            var requestType = method.GetParameters()[0].ParameterType;
            var responseType = method.ReturnType.GenericTypeArguments[0];

            // create grpc method 
            var grpcMethodFnc = typeof(SharedSetupUtil).GetMethod(nameof(SharedSetupUtil.CreateGrpcMethod)).MakeGenericMethod(requestType, responseType);
            var grpcMethod = grpcMethodFnc.Invoke(null, new object[] { serviceName, methodName });

            // create grpc method handler
            var grpcMethodHandlerFnc = typeof(GrpcServerBuilder).GetMethod(nameof(GrpcServerBuilder.GenerateGrpcUnaryMethodHandler)).
                MakeGenericMethod(serviceType, requestType, responseType);
            var grpcMethodHandler = grpcMethodHandlerFnc.Invoke(null, new object[] { method });

            // add method to services builder 
            var addMethodFnc = typeof(GrpcServerBuilder).GetMethod(nameof(GrpcServerBuilder.AddGrpcMethodToDefinitionBuilder)).
                MakeGenericMethod(requestType, responseType, serviceType);
            addMethodFnc.Invoke(null, new object[] { provider, builder, grpcMethod, grpcMethodHandler, cfg });

        }

        /// <summary>
        /// Generate grpc method handler
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Func<TService, TRequest, CancellationToken, Task<TResponse>> GenerateGrpcUnaryMethodHandler<TService, TRequest, TResponse>(MethodInfo method)
            where TService : class
            where TRequest : class
            where TResponse : class
        {
            var serviceParameter = Expression.Parameter(typeof(TService));
            var requestParameter = Expression.Parameter(typeof(TRequest));
            var ctParameter = Expression.Parameter(typeof(CancellationToken));
            var invocation = Expression.Call(serviceParameter, method, new[] { requestParameter, ctParameter });
            var func = Expression.Lambda<Func<TService, TRequest, CancellationToken, Task<TResponse>>>(invocation, false, new[] { serviceParameter, requestParameter, ctParameter }).Compile();
            return func;
        }

        /// <summary>
        /// Add grpc method to definition builder
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="provider"></param>
        /// <param name="builder"></param>
        /// <param name="method"></param>
        /// <param name="handler"></param>
        /// <param name="cfg"></param>
        public static void AddGrpcMethodToDefinitionBuilder<TRequest, TResponse, TService>(
            IServiceProvider provider, 
            ServerServiceDefinition.Builder builder,
            Method<TRequest, TResponse> method, 
            Func<TService, TRequest, CancellationToken, 
            Task<TResponse>> handler, 
            GrpcServerConfiguration cfg)
            where TService : class
            where TRequest : class
            where TResponse : class
        {

            // get host stats 
            var stats = provider.GetServices<GrpcHost>().FirstOrDefault(h => h.Name == cfg.Name)?.Stats;

            // add method 
            builder.AddMethod(method, async (req, responseStream, context) => {
                await RequestHandler.HandleRequest<TRequest, TResponse, TService>(provider, req, handler, cfg.Name, method.Name, context, responseStream);
            });
        }

    }
}

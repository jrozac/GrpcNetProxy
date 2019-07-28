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
using System.Diagnostics;

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

            // create builder
            var builder = ServerServiceDefinition.CreateBuilder();

            // get all methods to add to builder
            var allMethods = cfg.ServicesTypes.SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance)).ToList();

            // setup methods 
            allMethods.ForEach(m => GrpcServerBuilder.RegisterServiceMethodToGrpc(m, provider, logger, builder, cfg));

            // build services
            var services = builder.Build();

            // create grpc server 
            var server = new Grpc.Core.Server()
            {
                Ports = { { cfg.Connection.Url, cfg.Connection.Port, ServerCredentials.Insecure } },
                Services = { services }
            };

            // return server
            return server;

        }

        /// <summary>
        /// Register service methods to grpc
        /// </summary>
        /// <param name="method"></param>
        /// <param name="provider"></param>
        /// <param name="logger"></param>
        /// <param name="builder"></param>
        /// <param name="cfg"></param>
        public static void RegisterServiceMethodToGrpc(MethodInfo method, IServiceProvider provider,
            ILogger logger, ServerServiceDefinition.Builder builder, GrpcServerConfiguration cfg)
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
            addMethodFnc.Invoke(null, new object[] { provider, logger, builder,
                grpcMethod, grpcMethodHandler, cfg });

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
        /// <param name="logger"></param>
        /// <param name="builder"></param>
        /// <param name="method"></param>
        /// <param name="handler"></param>
        /// <param name="cfg"></param>
        public static void AddGrpcMethodToDefinitionBuilder<TRequest, TResponse, TService>(IServiceProvider provider, ILogger logger, ServerServiceDefinition.Builder builder,
            Method<TRequest, TResponse> method, Func<TService, TRequest, CancellationToken, Task<TResponse>> handler, GrpcServerConfiguration cfg)
            where TService : class
            where TRequest : class
            where TResponse : class
        {

            // add method 
            builder.AddMethod(method, async (req, responseStream, context) =>
            {
                // start measure time 
                var watch = Stopwatch.StartNew();

                // get log context 
                if (!string.IsNullOrEmpty(cfg.Options.ContextKey) && cfg.ContextSetter != null)
                {
                    // get context id 
                    var contextId = Enumerable.Range(0, context.RequestHeaders.Count).Select(i => context.RequestHeaders[i]).
                        FirstOrDefault(m => m.Key == cfg.Options.ContextKey)?.Value;
                    if (!string.IsNullOrEmpty(contextId))
                    {
                        cfg.ContextSetter.Invoke(contextId);
                    }
                }

                // log request start
                if (logger != null)
                {
                    logger.LogInformation("Start for action {action} on host {host} with request {@request}.",
                        $"{typeof(TService).Name}/{ method.Name}", cfg.Name, req);
                }

                // request start action
                cfg.OnRequestStart?.Invoke(logger, context, new RequestStartData
                {
                    MethodName = method.Name,
                    ServiceName = typeof(TService).Name,
                    HostName = cfg.Name,
                    Request = req
                });

                // execution inside new scope
                Exception ex = null;
                TResponse rsp = null;
                using (var scope = provider.CreateScope())
                {
                    try
                    {
                        // invokation
                        var svc = provider.GetRequiredService<TService>();
                        var rspTask = handler(svc, req, context.CancellationToken);
                        rsp = await rspTask;
                        await responseStream.WriteAsync(rsp);
                    }
                    catch (Exception e)
                    {
                        ex = e;
                        throw;
                    }
                    finally
                    {

                        // on end action ivoke
                        cfg.OnRequestEnd?.Invoke(logger, context, new RequestEndData
                        {
                            MethodName = method.Name,
                            ServiceName = typeof(TService).Name,
                            HostName = cfg.Name,
                            Request = req,
                            DurationMs = watch.ElapsedMilliseconds,
                            Exception = ex,
                            Response = rsp
                        });

                        // stop time measurement
                        watch.Stop();
                        var duration = watch.ElapsedMilliseconds;

                        // log end 
                        if (ex != null)
                        {
                            logger.LogInformation(ex, "End for action {action} on host {host} with request {@request}, response {@response} and duration {duration}.",
                                $"{typeof(TService).Name}/{method.Name}", cfg.Name, req, rsp, duration);
                        }
                        else
                        {
                            logger?.LogInformation("End for action {action} on host {host} with request {@request}, response {@response} and duration {duration}.",
                                $"{typeof(TService).Name}/{method.Name}", cfg.Name, req, rsp, duration);
                        }
                    }

                }
            });

        }

    }
}

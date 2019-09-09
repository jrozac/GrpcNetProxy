using Grpc.Core;
using GrpcNetProxy.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace GrpcNetProxy.Server
{

    /// <summary>
    /// Request handler
    /// </summary>
    internal static class RequestHandler
    {

        /// <summary>
        /// Handle request with response strem write
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="provider"></param>
        /// <param name="req"></param>
        /// <param name="handler"></param>
        /// <param name="hostName"></param>
        /// <param name="methodName"></param>
        /// <param name="context"></param>
        /// <param name="streamWriter"></param>
        /// <returns></returns>
        public static async Task<TResponse> HandleRequest<TRequest, TResponse, TService>(
            IServiceProvider provider,
            TRequest req,
            Func<TService, TRequest, CancellationToken, Task<TResponse>> handler,
            string hostName,
            string methodName,
            ServerCallContext context,
            IServerStreamWriter<TResponse> streamWriter)
            where TRequest : class
            where TResponse : class
            where TService : class
        {
            var rsp = await HandleRequest(provider, req, new Func<TService, TRequest, ServerCallContext, Task<TResponse>>(async (hsvc, hReq, hCtxt) => {
                return await handler(hsvc, hReq, hCtxt.CancellationToken);
            }), hostName, methodName, context, async (hRsp) => await streamWriter.WriteAsync(hRsp));
            return rsp;
        }

        /// <summary>
        /// Handle request
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="mainProvider"></param>
        /// <param name="req"></param>
        /// <param name="handler"></param>
        /// <param name="hostName"></param>
        /// <param name="methodName"></param>
        /// <param name="context"></param>
        /// <param name="onExectued"></param>
        /// <returns></returns>
        public static async Task<TResponse> HandleRequest<TRequest, TResponse, TService>(
            IServiceProvider mainProvider,
            TRequest req,
            Func<TService, TRequest, ServerCallContext, Task<TResponse>> handler,
            string hostName,
            string methodName,
            ServerCallContext context,
            Action<TResponse> onExectued = null)
            where TRequest : class
            where TResponse : class
            where TService : class
        {

            // run inside new scope
            using (var scope = mainProvider.CreateScope())
            {
                // get provider
                var provider = scope.ServiceProvider;

                // start measure time 
                var watch = Stopwatch.StartNew();

                // get host 
                var host = provider.GetServices<GrpcHost>().First(h => h.Name == hostName);
                var stats = host.Stats;
                var cfg = host.Configuration;

                // get logger if needed
                var logger = (cfg.Options?.LogRequests ?? false) ? provider.GetService<ILoggerFactory>()?.CreateLogger("GrpcServerRequests") : null;

                // get log context 
                if (!string.IsNullOrEmpty(cfg.Options.ContextKey) && cfg.DataHandlers.ContextSetter != null)
                {
                    // get context id 
                    var contextId = Enumerable.Range(0, context.RequestHeaders.Count).Select(i => context.RequestHeaders[i]).
                        FirstOrDefault(m => m.Key == cfg.Options.ContextKey)?.Value;
                    if (!string.IsNullOrEmpty(contextId))
                    {
                        cfg.DataHandlers.ContextSetter.Invoke(contextId);
                    }
                }

                // log request start
                logger?.LogInformation("Start for action {action} on host {host} with request {@request}.",
                    $"{typeof(TService).Name}/{ methodName}", hostName, req);

                // request start action
                cfg.DataHandlers.OnRequestStart?.Invoke(logger, context, new RequestStartData
                {
                    MethodName = methodName,
                    ServiceName = typeof(TService).Name,
                    HostName = hostName,
                    Request = req
                });

                // add stats 
                stats?.NewRequest(typeof(TService).Name, methodName, context);

                // execute request
                Exception ex = null;
                TResponse rsp = null;
                try
                {
                    // invokation
                    var svc = provider.GetRequiredService<TService>();
                    var rspTask = handler(svc, req, context);
                    rsp = await rspTask;

                    // on executed action
                    onExectued?.Invoke(rsp);

                    // return
                    return rsp;
                }
                catch (Exception e)
                {
                    // save error
                    stats?.RequestError(typeof(TService).Name, methodName, context, e.Message);
                    ex = e;
                    throw;
                }
                finally
                {

                    // on end action ivoke
                    cfg.DataHandlers.OnRequestEnd?.Invoke(logger, context, new RequestEndData
                    {
                        MethodName = methodName,
                        ServiceName = typeof(TService).Name,
                        HostName = hostName,
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
                            $"{typeof(TService).Name}/{methodName}", hostName, req, rsp, duration);
                    }
                    else
                    {
                        logger?.LogInformation("End for action {action} on host {host} with request {@request}, response {@response} and duration {duration}.",
                            $"{typeof(TService).Name}/{methodName}", hostName, req, rsp, duration);
                    }
                }

            }

        }

    }
}

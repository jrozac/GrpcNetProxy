using Grpc.Core;
using GrpcNetProxy.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Grpc client base. Generated client types will extend this class.
    /// </summary>
    public abstract class GrpcClientBase
    {
        /// <summary>
        /// Grpc channel
        /// </summary>
        private readonly GrpcChannelManager _channelManager;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Options
        /// </summary>
        private readonly GrpcClientConfiguration _configuration;

        /// <summary>
        /// Grpc methods 
        /// </summary>
        private readonly Dictionary<string, object> _grpcMethods;

        /// <summary>
        /// Constructor with required parameters
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="channelManager"></param>
        /// <param name="grpcMethods"></param>
        /// <param name="configuration"></param>
        protected GrpcClientBase(ILogger logger, GrpcChannelManager channelManager, Dictionary<string, object>  grpcMethods, GrpcClientConfiguration configuration)
        {
            // save fields
            _logger = logger;
            _channelManager = channelManager;
            _configuration = configuration;
            _grpcMethods = grpcMethods;
        }

        /// <summary>
        /// Call grpc method 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected async Task<TResponse> CallUnaryMethodAsync<TRequest, TResponse>(TRequest request, string serviceName, string methodName, CancellationToken ct)
            where TRequest : class
            where TResponse : class
        {

            // measure start
            var watch = Stopwatch.StartNew();

            // log start
            if(_configuration.ClientOptions.LogRequests && _logger != null)
            {
                _logger.LogDebug("Start for action {action} on client {client} with request {@request}.",
                    $"{serviceName}/{methodName}", _channelManager.Name, request);
            }
            
            // invoke start action
            _configuration.OnRequestStart?.Invoke(_logger, new RequestStartData {
                ServiceName = serviceName,
                MethodName = methodName,
                HostName = _channelManager.Name,
                Request = request });

            // execute call 
            Exception ex = null;
            TResponse rsp = null;
            try
            {
                // set context id data header
                var headers = new Metadata();
                var contextId = _configuration.ContextGetter?.Invoke();
                if(!string.IsNullOrWhiteSpace(_configuration.ClientOptions.ContextKey) && !string.IsNullOrWhiteSpace(contextId))
                {
                    headers.Add(new Metadata.Entry(_configuration.ClientOptions.ContextKey, contextId));
                }

                // pick invoker (channel, round robin)
                var invoker = _channelManager.NextInvoker();

                // make call
                var callOptions = new CallOptions(cancellationToken: ct, headers: headers);
                using (var call = invoker.AsyncUnaryCall(GetGrpcMethodDefinition<TRequest, TResponse>(serviceName, methodName), null, callOptions, request))
                {
                    rsp = await call.ResponseAsync.ConfigureAwait(false);
                }

            } catch(Exception e)
            {
                ex = e;
                throw;
            } finally
            {

                // request end action
                _configuration.OnRequestEnd?.Invoke(_logger, new RequestEndData
                {
                    ServiceName = serviceName,
                    MethodName = methodName,
                    HostName = _channelManager.Name,
                    Request = request,
                    Response = rsp,
                    DurationMs = watch.ElapsedMilliseconds,
                    Exception = ex
                });

                // stop time measuerement
                watch.Stop();
                var duration = watch.ElapsedMilliseconds;

                // log end 
                if (_configuration.ClientOptions.LogRequests && _logger != null)
                {
                    if(ex != null)
                    {
                        _logger.LogInformation(ex, "End for action {action} on client {client} with request {@request}, response {@response} and duration {duration}.",
                            $"{serviceName}/{methodName}", _channelManager.Name, request, rsp, duration);
                    } else
                    {
                        _logger.LogInformation("End for action {action} on client {client}  with request {@request}, response {@response} and duration {duration}.",
                            $"{serviceName}/{methodName}", _channelManager.Name, request, rsp, duration);
                    }
                }

            }

            // return
            return rsp;
        }

        /// <summary>
        /// Get grpc method definition
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private Method<TRequest, TResponse> GetGrpcMethodDefinition<TRequest, TResponse>(string serviceName, string methodName)
            where TRequest : class
            where TResponse : class
        {
            var key = $"{serviceName}/{methodName}";
            var method = _grpcMethods.ContainsKey(key) ? _grpcMethods[key] : null;
            if(method == null)
            {
                return null;
            }
            return method as Method<TRequest, TResponse>;
        }

        /// <summary>
        /// Configuration name
        /// </summary>
        internal string Name => _configuration.Name;
    }
}

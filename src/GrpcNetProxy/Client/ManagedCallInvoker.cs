using Grpc.Core;
using GrpcNetProxy.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Managed call invoker
    /// </summary>
    public class ManagedCallInvoker : CallInvoker
    {
        /// <summary>
        /// Round robin policy 
        /// </summary>
        private readonly RoundRobinPolicy<InvokerBundle> _roundRobin;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Configuration
        /// </summary>
        private readonly GrpcClientConfiguration _configuration;

        /// <summary>
        /// Custom invoker get delegate
        /// </summary>
        private readonly Func<object, string, string, string> _channelCustomSelector;

        /// <summary>
        /// Invokers 
        /// </summary>
        internal List<InvokerBundle> Invokers { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="clientConfiguration"></param>
        /// <param name="connectionInfoList"></param>
        /// <param name="channelCustomSelector"></param>
        public ManagedCallInvoker(ILogger logger, GrpcClientConfiguration clientConfiguration, 
            List<GrpcChannelConnectionData> connectionInfoList, Func<object, string, string, string> channelCustomSelector = null)
        {
            // set properteis
            _configuration = clientConfiguration;
            _logger = logger;
            _channelCustomSelector = channelCustomSelector;

            // create invoke bundles
            Invokers = connectionInfoList.Select(options => {
                var ch = new Channel(options.Url, options.Port, ChannelCredentials.Insecure);
                var inv = new DefaultCallInvoker(ch);
                var bundle = new InvokerBundle(ch, inv, options);
                return bundle;
            }).ToList();

            // setup round robin 
            _roundRobin = new RoundRobinPolicy<InvokerBundle>(Invokers, GetScore);
        }

        /// <summary>
        /// Get score for invoker
        /// </summary>
        /// <param name="ib"></param>
        /// <returns></returns>
        private int GetScore(InvokerBundle ib)
        {

            // not active 
            if (!ib.IsActive)
            {
                return int.MinValue;
            }

            // not connected physically
            if (ib.Channel.State != ChannelState.Idle && ib.Channel.State != ChannelState.Ready)
            {
                return int.MinValue;
            }

            // score offset pow
            int offset = 32768;

            // default score
            int score = 0;

            // apl online (16 points)
            if (ib.IsAplOnline)
            {
                score = score + offset * 16;
            }

            // error under limit (8 points)
            if (ib.ErrorsBelowThreshold)
            {
                score = score + offset * 8;
            }

            // return score
            return score;
        }

        /// <summary>
        /// Execute request
        /// </summary>
        /// <typeparam name="TAsyncRet"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="method"></param>
        /// <param name="host"></param>
        /// <param name="options"></param>
        /// <param name="request"></param>
        /// <param name="executeFnc"></param>
        /// <param name="retTaskFnc"></param>
        /// <returns></returns>
        private TAsyncRet ExecuteAsync<TAsyncRet, TRequest, TResponse> (Method<TRequest, TResponse> method, string host, 
            CallOptions options, TRequest request,
            Func<CallInvoker, Method<TRequest, TResponse>, string, CallOptions, TRequest, TAsyncRet> executeFnc, 
            Func<TAsyncRet, Task<TResponse>> retTaskFnc)
        {

            // measure start
            var watch = Stopwatch.StartNew();

            // log start
            if (_configuration.Options.LogRequests && _logger != null)
            {
                _logger.LogDebug("Start for action {action} on client {client} with request {@request}.",
                    $"{method.ServiceName}/{method.Name}", _configuration.Name, request);
            }

            // invoke start action
            _configuration.DataHandlers.OnRequestStart?.Invoke(_logger, new RequestStartData
            {
                ServiceName = method.ServiceName,
                MethodName = method.Name,
                HostName = _configuration.Name,
                Request = request
            });

            // pick invoker (channel, round robin)
            string invokerId = _channelCustomSelector?.Invoke(request, method.ServiceName, method.Name) ?? null;
            var bundle = invokerId != null ? Invokers.FirstOrDefault(b => b.Id.Equals(invokerId, StringComparison.InvariantCultureIgnoreCase)) : null;
            bundle = bundle ?? _roundRobin.GetNext();
            var invoker = bundle.Invoker;

            // set context id data header
            options = TryFilterCallOptions(options);

            // add invoke count
            bundle.AddInvokeCount();

            // execute action
            TAsyncRet exRet;
            try
            {
                exRet = executeFnc.Invoke(invoker, method, host, options, request);
            } catch(Exception e)
            {
                var exTaskErr = Task.FromException<TResponse>(e);
                exTaskErr.ContinueWith((t) => OnExecuteCompleteHandler(watch, method, request, bundle, t));
                throw;
            }
            
            // after execution 
            var exTask = retTaskFnc(exRet);
            if(exTask != null)
            {
                exTask.ContinueWith((t) => OnExecuteCompleteHandler(watch, method, request, bundle, t));
            }

            // return
            return exRet;
        }

        /// <summary>
        /// On execution completed
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="watch"></param>
        /// <param name="method"></param>
        /// <param name="request"></param>
        /// <param name="bundle"></param>
        /// <param name="t"></param>
        private void OnExecuteCompleteHandler<TRequest,TResponse>(Stopwatch watch, Method<TRequest, TResponse> method, 
            TRequest request, InvokerBundle bundle, Task<TResponse> t)
        {
            // task status
            TaskStatus status = t.Status;

            // get execution duration
            watch.Stop();
            var duration = watch.ElapsedMilliseconds;

            // invoker status update 
            if (status == TaskStatus.RanToCompletion)
            {
                bundle.ResetError();
            }
            else
            {
                bundle.AddError();
            }

            // log end 
            if (_configuration.Options.LogRequests && _logger != null)
            {
                if (status == TaskStatus.RanToCompletion)
                {
                    _logger.LogInformation("End for action {action} on client {client}  with request {@request}, response {@response} and duration {duration}.",
                        $"{method.ServiceName}/{method.Name}", _configuration.Name, request, t.Result, duration);
                }
                else
                {
                    _logger.LogError(t.Exception, "End for failed action {action} on client {client} with request {@request} and duration {duration}.",
                        $"{method.ServiceName}/{method.Name}", _configuration.Name, request, duration);
                }
            }

            // request end action
            try
            {
                _configuration.DataHandlers.OnRequestEnd?.Invoke(_logger, new RequestEndData
                {
                    ServiceName = method.ServiceName,
                    MethodName = method.Name,
                    HostName = _configuration.Name,
                    Request = request,
                    Response = t.Result,
                    DurationMs = duration,
                    Exception = t.Exception
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to execute reqeust end action.");
            }
        }


        /// <summary>
        /// Filter call options (add context id to header)
        /// </summary>
        /// <param name="callOptions"></param>
        private CallOptions TryFilterCallOptions(CallOptions callOptions)
        {

            try
            {

                // new options 
                bool newOptions = false;

                // get headers
                var headers = callOptions.Headers ?? new Metadata();

                // set context id 
                var contextId = _configuration.DataHandlers.ContextGetter?.Invoke();
                if (!string.IsNullOrWhiteSpace(_configuration.Options.ContextKey) && !string.IsNullOrWhiteSpace(contextId))
                {
                    headers.Add(new Metadata.Entry(_configuration.Options.ContextKey, contextId));
                    newOptions = true;
                }

                // set timeout deadline if not set
                if(callOptions.Deadline == null)
                {
                    newOptions = true;
                }
                var deadline = callOptions.Deadline ?? DateTime.UtcNow.AddMilliseconds(_configuration.Options.TimeoutMs);

                // new options required
                if(newOptions)
                {
                    var newCallOptions = new CallOptions(headers, deadline, callOptions.CancellationToken, callOptions.WriteOptions, callOptions.PropagationToken, callOptions.Credentials);
                    return newCallOptions;
                }

                // return existing options
                return callOptions;

            } catch(Exception e)
            {
                _logger.LogError(e,"Failed to filter call options");
                return callOptions;
            }
        }

        /// <summary>
        /// Async unary call 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="method"></param>
        /// <param name="host"></param>
        /// <param name="options"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return ExecuteAsync(method, host, options, request,
                (localInvoker, localMethod, localHost, localOptions, localRequest)
                    => localInvoker.AsyncUnaryCall(localMethod, localHost, localOptions, localRequest), r => r.ResponseAsync);
        }
        
        /// <summary>
        /// Blocking unary calls
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="method"></param>
        /// <param name="host"></param>
        /// <param name="options"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return ExecuteAsync(method, host, options, request,
                (localInvoker, localMethod, localHost, localOptions, localRequest)
                    => localInvoker.BlockingUnaryCall(localMethod, localHost, localOptions, localRequest), r => Task.FromResult(r));
        }

        #region NotSupported

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            throw new NotSupportedException();
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            throw new NotSupportedException();
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            throw new NotSupportedException();
        }

        #endregion

    }
}

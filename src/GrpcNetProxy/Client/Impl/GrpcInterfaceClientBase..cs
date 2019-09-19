using Grpc.Core;
using GrpcNetProxy.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Grpc client base. Generated client types will extend this class.
    /// </summary>
    public abstract class GrpcInterfaceClientBase : IGrpcClient
    {
        /// <summary>
        /// Grpc channel
        /// </summary>
        private readonly CallInvoker _invoker;

        /// <summary>
        /// Grpc methods 
        /// </summary>
        private readonly Dictionary<string, object> _grpcMethods;

        /// <summary>
        /// Constructor with required parameters
        /// </summary>
        /// <param name="invoker"></param>
        /// <param name="name"></param>
        protected GrpcInterfaceClientBase(CallInvoker invoker, string name)
        {
            // save fields
            _invoker = invoker;
            Name = name;

            // make grpc methods
            var serviceType = GetType().GetInterfaces().First(i => i != typeof(IGrpcClient));
            _grpcMethods = CreateGrpcMethodsForInterfaceType(serviceType);
        }

        /// <summary>
        /// Call grpc method
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        protected async Task<TResponse> CallUnaryMethodAsync<TRequest, TResponse>(TRequest request, CancellationToken ct, string serviceName, string methodName)
            where TRequest : class
            where TResponse : class
        {
            var callOptions =  new CallOptions(cancellationToken: ct);
            using (var call = _invoker.AsyncUnaryCall(GetGrpcMethodDefinition<TRequest, TResponse>(serviceName, methodName), null, callOptions, request))
            {
                return await call.ResponseAsync.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get grpc methods for service type
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private static Dictionary<string, object> CreateGrpcMethodsForInterfaceType(Type serviceType)
        {
            return serviceType.GetMethods().ToDictionary(m => $"{m.DeclaringType.Name}/{m.Name}", m => {
                var requestType = m.GetParameters()[0].ParameterType;
                var responseType = m.ReturnType.GenericTypeArguments[0];
                var grpcMethodFnc = typeof(GrpcMethodFactoryUtil).GetMethod(nameof(GrpcMethodFactoryUtil.CreateGrpcMethod)).MakeGenericMethod(requestType, responseType);
                var grpcMethod = grpcMethodFnc.Invoke(null, new object[] { serviceType.Name, m.Name });
                return grpcMethod;
            });
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
        public string Name { get; }

    }
}

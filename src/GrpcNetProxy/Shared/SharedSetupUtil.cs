using Grpc.Core;
using GrpcNetProxy.Serialization;
using System;

namespace GrpcNetProxy.Shared
{
    /// <summary>
    /// Setup utility generic
    /// </summary>
    internal static class SharedSetupUtil
    {

        /// <summary>
        /// Create grpc method with default serializer
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static Method<TRequest, TResponse> CreateGrpcMethod<TRequest, TResponse>(string serviceName, string methodName)
            where TRequest : class
            where TResponse : class
        {
            return CreateGrpcMethodWithCustomSerializer<TRequest, TResponse, ProtoBufSerializer>(serviceName, methodName);
        }

        /// <summary>
        /// Create grpc method
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TSerializer"></typeparam>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static Method<TRequest, TResponse> CreateGrpcMethodWithCustomSerializer<TRequest, TResponse, TSerializer>(string serviceName, string methodName)
            where TRequest : class
            where TResponse : class
            where TSerializer : ISerializer
        {
            // create serializer 
            var serializer = Activator.CreateInstance<TSerializer>();

            // create method
            var method = new Method<TRequest, TResponse>(
                type: MethodType.DuplexStreaming,
                serviceName: serviceName,
                name: methodName,
                requestMarshaller: Marshallers.Create(
                    obj => serializer.Serialize(obj),
                    bytes => serializer.Deserialize<TRequest>(bytes)),
                responseMarshaller: Marshallers.Create(
                    obj => serializer.Serialize(obj),
                    bytes => serializer.Deserialize<TResponse>(bytes)));

            // return
            return method;
        }
    }
}

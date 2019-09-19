using System;
using System.Linq;
using GrpcNetProxy.Generics;
using Grpc.Core;

namespace GrpcNetProxy.Client
{
    /// <summary>
    /// Grpc client builder
    /// </summary>
    internal static class GrpcClientFactoryUtil
    {

        /// <summary>
        /// Build client
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="invoker"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        internal static TService Create<TService>(CallInvoker invoker, GrpcClientConfiguration configuration)
            where TService : class
        {

            // protofiles generated client 
            if (typeof(TService).GetInheritanceHierarchy().Any(t => t == typeof(ClientBase)))
            {
                return CreateProtoClient<TService>(invoker, configuration.Name);
            }
            else
            { // internally build client from interface
                return CreateInterfaceClient<TService>(invoker, configuration.Name);
            }

        }

        /// <summary>
        /// Create proto client (generated from proto file)
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="invoker"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static TService CreateProtoClient<TService>(CallInvoker invoker, string name)
            where TService : class
        {
            // create type 
            var newType = GrpcProtoClientTypeBuilder.Create<TService>();

            // create instance 
            var client = (TService)Activator.CreateInstance(newType, invoker);

            // set name 
            client.GetType().GetProperty("Name").SetMethod.Invoke(client, new[] { name });

            // return
            return client;
        }

        /// <summary>
        /// Create client from interface
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="invoker"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static TService CreateInterfaceClient<TService>(CallInvoker invoker, string name)
            where TService : class
        {
            // create type 
            var newType = GrpcInterfaceClientTypeBuilder.Create<TService>();

            // create service and return
            return (TService)Activator.CreateInstance(newType, invoker, name);
        }
    }

}

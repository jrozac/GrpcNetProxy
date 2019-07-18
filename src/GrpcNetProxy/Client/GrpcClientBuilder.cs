using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using GrpcNetProxy.Shared;
using System.Collections.Generic;
using System.Linq;

namespace GrpcNetProxy.Client
{
    /// <summary>
    /// Grpc client builder
    /// </summary>
    internal static class GrpcClientBuildercs
    {

        /// <summary>
        /// Build client 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="provider"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        internal static TService Build<TService>(IServiceProvider provider, GrpcClientConfiguration configuration)
            where TService : class
        {

            // create type 
            var newType = GrpcClientTypeBuilder.Create<TService>();

            // create logger
            var logger = provider.GetService<ILoggerFactory>()?.CreateLogger("GrpcClientRequests");

            // get channels proxy
            var channelsProxy = provider.GetRequiredService<GrpcChannelManager<TService>>() as GrpcChannelManager;

            // create grpc methods
            var grpcMethods = GetGrpcMethodsForInterfaceType(typeof(TService));

            // create service and return
            return (TService)Activator.CreateInstance(newType, logger, channelsProxy, grpcMethods, configuration);

        }

        /// <summary>
        /// Get grpc methods for service type
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private static Dictionary<string, object> GetGrpcMethodsForInterfaceType(Type serviceType)
        {
            return serviceType.GetMethods().ToDictionary(m => $"{m.DeclaringType.Name}/{m.Name}", m => {
                var requestType = m.GetParameters()[0].ParameterType;
                var responseType = m.ReturnType.GenericTypeArguments[0];
                var grpcMethodFnc = typeof(SharedSetupUtil).GetMethod(nameof(SharedSetupUtil.CreateGrpcMethod)).MakeGenericMethod(requestType, responseType);
                var grpcMethod = grpcMethodFnc.Invoke(null, new object[] { serviceType.Name, m.Name });
                return grpcMethod;
            });
        }

    }
}

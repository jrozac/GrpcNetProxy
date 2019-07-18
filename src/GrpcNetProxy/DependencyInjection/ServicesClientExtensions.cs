using GrpcNetProxy.Client;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GrpcNetProxy.DependencyInjection
{

    /// <summary>
    /// Services dependency injection grpc client extensions 
    /// </summary>
    public static class ServicesClientExtensions
    {
        /// <summary>
        /// Add grpc client
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="collection"></param>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcClient<TService>(this IServiceCollection collection, Action<ClientConfigurator> cfg)
            where TService : class
        {

            // apply configuration
            ClientConfigurator configurator = new ClientConfigurator();
            cfg?.Invoke(configurator);

            // add channels proxy
            collection.AddSingleton(provider => new GrpcChannelManager<TService>(configurator.ChannelManagerConfiguration));

            // setup client 
            collection.AddSingleton(provider => GrpcClientBuildercs.Build<TService>(provider, configurator.ClientConfiguration));

            // return 
            return collection;

        }

        /// <summary>
        /// Get grpc client channel manager
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static GrpcChannelManager GetGrpcClientChannelManager<TService>(this IServiceProvider provider) 
            where TService : class
        {
            return provider.GetRequiredService<GrpcChannelManager<TService>>() as GrpcChannelManager;
        }
    }
}

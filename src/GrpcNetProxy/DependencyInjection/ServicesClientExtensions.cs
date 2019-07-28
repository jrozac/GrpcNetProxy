using GrpcNetProxy.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

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
        public static IServiceCollection AddGrpcClient(this IServiceCollection collection, Action<ClientConfigurator> cfg)
        {

            // apply configuration
            ClientConfigurator configurator = new ClientConfigurator();
            cfg?.Invoke(configurator);

            // add channels proxy
            collection.AddSingleton(provider => new GrpcChannelManager(configurator.ClientConfiguration.Name, configurator.ChannelManagerConfiguration));
            
            // add services
            configurator.RegisteredServices.ForEach(svcType => {

                // get build method
                var methodBuild = typeof(GrpcClientBuilder).
                    GetMethod(nameof(GrpcClientBuilder.Build), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(svcType);

                // add client
                collection.AddSingleton(svcType, provider => methodBuild.Invoke(null, new object[] { provider, configurator.ClientConfiguration }));
            });

            // return 
            return collection;

        }

        /// <summary>
        /// Get grpc channel manager
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GrpcChannelManager GetGrpcClientChannelManager(this IServiceProvider provider, string name = "Default")
        {
            return provider.GetServices<GrpcChannelManager>().First(m => m.Name == name);
        }

        /// <summary>
        /// Get grpc client service.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TService GetGrpcClientService<TService>(this IServiceProvider provider, string name = "Default")
            where TService : class
        {
            return provider.GetServices<TService>().First(m => (m as GrpcClientBase)?.Name == name);
        }

    }
}

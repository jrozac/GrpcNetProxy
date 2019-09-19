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
        /// Setup custom client
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcClient(this IServiceCollection collection, Action<ClientConfigurator> cfg)
        {

            // apply configuration
            ClientConfigurator configurator = new ClientConfigurator();
            collection.AddGrpcClient(configurator, cfg);

            // return
            return collection;
        }

        /// <summary>
        /// Add grpc client
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="collection"></param>
        /// <param name="configurator"></param>
        /// <param name="setup"></param>
        /// <returns></returns>
        internal static IServiceCollection AddGrpcClient(this IServiceCollection collection, ClientConfigurator configurator, 
            Action<ClientConfigurator> setup = null)
        {

            // apply configuration
            setup?.Invoke(configurator);

            // add client manager
            collection.AddSingleton(provider => new GrpcClientManager(configurator.ClientConfiguration, provider));

            // add services
            configurator.RegisteredServices.ForEach(svcType => {

                // get build method
                var methodBuild = typeof(GrpcClientFactoryUtil).
                    GetMethod(nameof(GrpcClientFactoryUtil.Create), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(svcType);

                // add client
                collection.AddSingleton(svcType, provider => {
                    var invoker = provider.GetServices<GrpcClientManager>().First(m => m.Name == configurator.ClientConfiguration.Name).GetInvoker();
                    return methodBuild.Invoke(null, new object[] { invoker, configurator.ClientConfiguration });
                });
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
        public static GrpcClientManager GetGrpcClientManager(this IServiceProvider provider, string name = "Default")
        {
            return provider.GetServices<GrpcClientManager>().First(m => m.Name == name);
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
            return provider.GetServices<TService>().First(m => (m as IGrpcClient)?.Name == name);
        }

    }
}

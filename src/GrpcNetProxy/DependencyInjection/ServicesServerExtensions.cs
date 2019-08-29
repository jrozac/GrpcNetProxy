using GrpcNetProxy.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace GrpcNetProxy.DependencyInjection
{

    /// <summary>
    /// Grpc server dependency injection extensions
    /// </summary>
    public static class ServicesServerExtensions
    {

        /// <summary>
        /// Add grpc server
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcServer(this IServiceCollection collection, Action<ServerConfigurator> cfg)
        {

            // apply configuration
            ServerConfigurator configurator = new ServerConfigurator();
            cfg?.Invoke(configurator);

            // register host
            collection.AddSingleton(provider => new GrpcHost(provider, configurator.Configuration));
            return collection;
        }

        /// <summary>
        /// Add grpc hosted service
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcHostedService(this IServiceCollection collection, string name = "Default")
        {
            collection.AddSingleton<IHostedService, GrpcHostedService>(provider => 
                new GrpcHostedService(provider.GetServices<GrpcHost>().First(h => h.Name == name)));
            return collection;
        }

        /// <summary>
        /// Add grpc hosted service with configuration
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcHostedService(this IServiceCollection collection, Action<ServerConfigurator> cfg)
        {
            // apply configuration
            ServerConfigurator configurator = new ServerConfigurator();
            collection.AddGrpcHostedService(configurator, cfg);

            // return
            return collection;
        }

        /// <summary>
        /// Add grpc hosted service with configurator and configuration
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="configurator"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        internal static IServiceCollection AddGrpcHostedService(this IServiceCollection collection, ServerConfigurator configurator, 
            Action<ServerConfigurator> setupAction = null)
        {
            // apply configuration
            setupAction?.Invoke(configurator);

            // register host
            collection.AddSingleton(provider => new GrpcHost(provider, configurator.Configuration));

            // add hosted service
            collection.AddSingleton<IHostedService, GrpcHostedService>(provider =>
                new GrpcHostedService(provider.GetServices<GrpcHost>().First(h => h.Name == configurator.Configuration.Name)));

            // return
            return collection;
        }

        /// <summary>
        /// Gets grpc host by name
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GrpcHost GetGrpcHost(this IServiceProvider services, string name = "Default")
        {
            return services.GetServices<GrpcHost>().First(h => h.Name == name);
        }

    }
}

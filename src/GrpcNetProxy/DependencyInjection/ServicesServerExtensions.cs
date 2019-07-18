using GrpcNetProxy.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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
        /// <returns></returns>
        public static IServiceCollection AddGrpcHostedService(this IServiceCollection collection)
        {
            collection.AddSingleton<IHostedService, GrpcHostedService>(provider => new GrpcHostedService(provider.GetService<GrpcHost>()));
            return collection;
        }

        /// <summary>
        /// Grpc host getter
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static GrpcHost GetGrpcHost(this IServiceProvider provider)
        {
            return provider.GetRequiredService<GrpcHost>();
        }

    }
}

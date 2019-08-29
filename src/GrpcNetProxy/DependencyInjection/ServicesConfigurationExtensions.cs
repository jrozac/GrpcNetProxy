using GrpcNetProxy.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GrpcNetProxy.DependencyInjection
{

    /// <summary>
    /// Services configuration extensions
    /// </summary>
    public static class ServicesConfigurationExtensions
    {

        /// <summary>
        /// Add grpc
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="cfgFilePath"></param>
        /// <param name="setup"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureGrpc(this IServiceCollection collection, string cfgFilePath, Action<GrpcConfigurators> setup = null)
        {
            // load configuration
            var cfg = ConfigurationLoader.LoadConfiguration(cfgFilePath);

            // apply custom setup
            setup?.Invoke(cfg);

            // setup servers 
            cfg.Servers?.ForEach(serverCfg => {

                collection.AddGrpcHostedService(serverCfg);
            });

            // setup clients 
            cfg.Clients.ForEach(clientCfg => {
                collection.AddGrpcClient(clientCfg);
            });

            // return collection
            return collection;

        }
    }
}

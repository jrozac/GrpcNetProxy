using GrpcNetProxy.Client;
using GrpcNetProxy.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GrpcNetProxy.Configuration
{

    /// <summary>
    /// Configuration loader
    /// </summary>
    internal static class ConfigurationLoader
    {

        /// <summary>
        /// Load configuration
        /// </summary>
        /// <param name="cfgFilePath"></param>
        /// <returns></returns>
        public static GrpcConfigurators LoadConfiguration(string cfgFilePath)
        {
            // check for file existance
            if(!File.Exists(cfgFilePath))
            {
                throw new ArgumentException("File path is invalid.");
            }

            // load file
            var jsonRaw = File.ReadAllText(cfgFilePath);
            var configuration = JsonConvert.DeserializeObject<GrpcConfiguration>(jsonRaw);

            // create configurators and return
            var clients = configuration.Clients?.ToClientConfigurators() ?? new List<ClientConfigurator>();
            var servers = configuration.Servers?.ToServerConfigurators() ?? new List<ServerConfigurator>();
            return new GrpcConfigurators(servers, clients);
        }

        /// <summary>
        /// Get server configurators
        /// </summary>
        /// <param name="serversConfigurations"></param>
        /// <returns></returns>
        public static List<ServerConfigurator> ToServerConfigurators(this List<ServerConfiguration> serversConfigurations)
        {

            return serversConfigurations?.Select(config => {

                // create builder
                ServerConfigurator builder = new ServerConfigurator();

                // set name 
                if (!string.IsNullOrWhiteSpace(config.Name))
                {
                    builder.SetName(config.Name);
                }

                // enble status service
                if (config.EnableStatusService)
                {
                    builder.AddStatusService();
                }

                // set server options
                if (config.Options != null)
                {
                    builder.SetOptions(config.Options);
                };

                // get services
                config.Services?.Distinct().ToList().ForEach(svc => {
                    builder.AddService(svc);
                });

                // set host connection
                if (config.Host != null)
                {
                    builder.SetConnection(config.Host);
                }

                // return
                return builder;

            }).ToList();

        }

        /// <summary>
        /// Get client configurators
        /// </summary>
        /// <param name="clientConfigurations"></param>
        /// <returns></returns>
        public static List<ClientConfigurator> ToClientConfigurators(this List<ClientConfiguration> clientConfigurations)
        {
            return clientConfigurations?.Select(config =>
            {

                // create builder
                ClientConfigurator builder = new ClientConfigurator();

                // set name 
                if (!string.IsNullOrWhiteSpace(config.Name))
                {
                    builder.SetName(config.Name);
                }

                // add hosts 
                config.Hosts?.ForEach(host => builder.AddHost(host));

                // set client options
                if (config.Options != null)
                {
                    builder.SetClientOptions(config.Options);
                };

                // get services
                config.Services?.Distinct().ToList().ForEach(svc => {
                    builder.AddService(svc);
                });

                // return
                return builder;

            }).ToList();
    
        }

    }
}

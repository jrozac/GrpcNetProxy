using GrpcNetProxy.Client;
using GrpcNetProxy.Server;
using System.Collections.Generic;
using System.Linq;

namespace GrpcNetProxy.Configuration
{

    /// <summary>
    /// Grpc configurators
    /// </summary>
    public class GrpcConfigurators
    {

        /// <summary>
        /// Internal constructor with servers and client configurators as arguments.
        /// </summary>
        /// <param name="servers"></param>
        /// <param name="clients"></param>
        internal GrpcConfigurators(List<ServerConfigurator> servers, List<ClientConfigurator> clients)
        {
            Servers = servers;
            Clients = clients;
        }

        /// <summary>
        /// Servers 
        /// </summary>
        internal List<ServerConfigurator> Servers {get;set;}

        /// <summary>
        /// Clients 
        /// </summary>
        internal List<ClientConfigurator> Clients { get; set; }

        /// <summary>
        /// Client selector
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ClientConfigurator Client(string name = "Default") 
            => Clients.FirstOrDefault(c => c.ClientConfiguration.Name == name);

        /// <summary>
        /// Server selector
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ServerConfigurator Server(string name = "Default")
            => Servers.FirstOrDefault(s => s.Configuration.Name == name);

    }
}

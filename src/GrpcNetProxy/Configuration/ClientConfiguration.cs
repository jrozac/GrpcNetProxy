using GrpcNetProxy.Client;
using System.Collections.Generic;

namespace GrpcNetProxy
{

    /// <summary>
    /// Grpc client configuration
    /// </summary>
    public class ClientConfiguration
    {

        /// <summary>
        /// Client name
        /// </summary>
        public string Name { get; set; } = "Default";

        /// <summary>
        /// Hosts 
        /// </summary>
        public List<GrpcChannelConnectionData> Hosts { get; set; }

        /// <summary>
        /// Client options
        /// </summary>
        public GrpcClientOptions Options { get; set; }

        /// <summary>
        /// Services 
        /// </summary>
        public List<string> Services { get; set; }

    }
}

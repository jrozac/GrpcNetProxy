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
        /// Monitor interval
        /// </summary>
        public int MonitorInterval { get; set; } = 1 * 60 * 1000;

        /// <summary>
        /// Hosts 
        /// </summary>
        public List<GrpcChannelConnectionData> Hosts { get; set; }

        /// <summary>
        /// Status service
        /// </summary>
        public bool EnableStatusService { get; set; }

        /// <summary>
        /// Client options
        /// </summary>
        public GrpcClientOptions Options { get; set; }
    }
}

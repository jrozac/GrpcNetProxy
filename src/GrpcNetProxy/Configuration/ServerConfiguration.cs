using GrpcNetProxy.Server;
using System.Collections.Generic;

namespace GrpcNetProxy
{

    /// <summary>
    /// Grpc server configuration
    /// </summary>
    public class ServerConfiguration
    {

        /// <summary>
        /// Client name
        /// </summary>
        public string Name { get; set; } = "Default";

        /// <summary>
        /// Status service
        /// </summary>
        public bool EnableStatusService { get; set; }

        /// <summary>
        /// Server options
        /// </summary>
        public GrpcServerOptions Options { get; set; }

        /// <summary>
        /// Set host data
        /// </summary>
        public GrpcServerConnectionData Host { get; set; }

        /// <summary>
        /// Services
        /// </summary>
        public List<string> Services { get; set; }

    }
}

using System.Collections.Generic;

namespace GrpcNetProxy
{

    /// <summary>
    /// Grpc configuration
    /// </summary>
    public class GrpcConfiguration
    {

        /// <summary>
        /// Grpc clients
        /// </summary>
        public List<ClientConfiguration> Clients { get; set; }

        /// <summary>
        /// Grpc servers
        /// </summary>
        public List<ServerConfiguration> Servers { get; set; }

    }
}

using System.Collections.Generic;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Grpc client configuration
    /// </summary>
    public class GrpcClientConfiguration
    {

        /// <summary>
        /// Internal constructor prevers external creation
        /// </summary>
        internal GrpcClientConfiguration() { }

        /// <summary>
        /// Client options 
        /// </summary>
        public GrpcClientOptions Options { get; set; } = new GrpcClientOptions();

        /// <summary>
        /// Data handlers
        /// </summary>
        internal GrpcClientDataHandlers DataHandlers { get; } = new GrpcClientDataHandlers();

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Channels connections 
        /// </summary>
        public List<GrpcChannelConnectionData> Hosts = new List<GrpcChannelConnectionData>();

    }
}

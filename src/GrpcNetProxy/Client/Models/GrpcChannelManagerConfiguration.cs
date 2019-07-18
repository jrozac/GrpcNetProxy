using System.Collections.Generic;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Grpc channels configuration
    /// </summary>
    internal class GrpcChannelManagerConfiguration
    {
        /// <summary>
        /// Channels options 
        /// </summary>
        public List<GrpcChannelConnectionData> ChannelsOptions = new List<GrpcChannelConnectionData>();
    }
}

using System;
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

        /// <summary>
        /// Status service type
        /// </summary>
        public bool StatusServiceEnabled { get; set; }

        /// <summary>
        /// Monitor interval ms
        /// </summary>
        public int MonitorInterval { get; set; } = 1 * 60  * 1000;
    }
}

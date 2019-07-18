using Grpc.Core;

namespace GrpcNetProxy.Client
{
    /// <summary>
    /// Grpc channel status
    /// </summary>
    public class GrpcChannelStatus
    {
        /// <summary>
        /// Channel options 
        /// </summary>
        public GrpcChannelConnectionData Options {get;set;}

        /// <summary>
        /// Channel state
        /// </summary>
        public ChannelState State { get; set; }
    }
}

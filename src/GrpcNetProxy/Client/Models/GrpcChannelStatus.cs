using Grpc.Core;

namespace GrpcNetProxy.Client
{
    /// <summary>
    /// Grpc channel status
    /// </summary>
    public class GrpcChannelStatus
    {

        /// <summary>
        /// Channel id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Channel options 
        /// </summary>
        public GrpcChannelConnectionData Options {get;set;}

        /// <summary>
        /// Channel state
        /// </summary>
        public ChannelState State { get; set; }

        /// <summary>
        /// error threshold under limit
        /// </summary>
        public bool ErrorsBelowThreshold { get; set; }

        /// <summary>
        /// Error count
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// Is active status
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Is applicatively online
        /// </summary>
        public bool IsAplOnline { get; set; }

        /// <summary>
        /// Invoke count
        /// </summary>
        public int InvokeCount { get; set; }
    }
}

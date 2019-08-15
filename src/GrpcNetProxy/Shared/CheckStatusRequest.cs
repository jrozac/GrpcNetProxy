namespace GrpcNetProxy.Shared
{
    /// <summary>
    /// Check status request
    /// </summary>
    [ProtoBuf.ProtoContract]
    public class CheckStatusRequest
    {
        /// <summary>
        /// Channel id 
        /// </summary>
        [ProtoBuf.ProtoMember(1)]
        public string ChannelId { get; set; }
    }
}

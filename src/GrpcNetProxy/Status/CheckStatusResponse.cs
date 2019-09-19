namespace GrpcNetProxy.Status
{
    /// <summary>
    /// Check status response
    /// </summary>
    [ProtoBuf.ProtoContract]
    public class CheckStatusResponse
    {
        /// <summary>
        /// Status
        /// </summary>
        [ProtoBuf.ProtoMember(1)]
        public bool Status { get; set; }
    }
}

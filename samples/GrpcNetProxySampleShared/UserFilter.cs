namespace GrpcNetProxySampleShared
{

    /// <summary>
    /// User filter with protobuf contract attributes
    /// </summary>
    [ProtoBuf.ProtoContract]
    public class UserFilter
    {
        /// <summary>
        /// User id
        /// </summary>
        [ProtoBuf.ProtoMember(1)]
        public string Id { get; set; }
    }
}

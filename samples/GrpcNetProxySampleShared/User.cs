namespace GrpcNetProxySampleShared
{

    /// <summary>
    /// User
    /// </summary>
    [ProtoBuf.ProtoContract]
    public class User
    {
        /// <summary>
        /// Name
        /// </summary>
        [ProtoBuf.ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        [ProtoBuf.ProtoMember(2)]
        public string Id { get; set; }
    }
}

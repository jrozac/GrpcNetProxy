namespace GrpcNetProxySampleShared
{
    [ProtoBuf.ProtoContract]
    public class User
    {
        [ProtoBuf.ProtoMember(1)]
        public string Name { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public string Id { get; set; }
    }
}

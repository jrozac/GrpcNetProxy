namespace GrpcNetProxySampleShared
{
    [ProtoBuf.ProtoContract]
    public class UserFilter
    {
        [ProtoBuf.ProtoMember(1)]
        public string Id { get; set; }
    }
}

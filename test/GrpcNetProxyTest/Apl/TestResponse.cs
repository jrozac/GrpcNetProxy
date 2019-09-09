namespace GrpcNetProxyTest.Apl
{

    /// <summary>
    /// Test response
    /// </summary>
    [ProtoBuf.ProtoContract]
    public class TestResponse
    {
        /// <summary>
        /// Response id 
        /// </summary>
        [ProtoBuf.ProtoMember(1)]
        public string Id { get; set; }

        /// <summary>
        /// Constructor count
        /// </summary>
        [ProtoBuf.ProtoMember(2)]
        public int ConstructorInvokeCount { get; set; }
    }
}

using System;

namespace GrpcNetProxyTest.Apl
{
    /// <summary>
    /// Test request 
    /// </summary>
    [ProtoBuf.ProtoContract]
    public class TestRequest
    {
        /// <summary>
        /// Request id 
        /// </summary>
        [ProtoBuf.ProtoMember(1)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}

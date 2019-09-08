using GrpcNetProxyTest.Scenarios;
using GrpcNetProxyTest.Setup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GrpcNetProxyTest
{

    /// <summary>
    /// Test for usage of services (client and server) generated through proto files
    /// </summary>
    [TestClass]
    public class ProtogenServerTest : TestBase<DefaultScenario>
    {

        /// <summary>
        /// Test service generated from proto file hosted tthrough grpc proxy is properly serving generated client
        /// </summary>
        [TestMethod]
        public void TestProtogenServiceIsCalledFromGeneratedClient()
        {
            using(var scneario = Setup())
            {

                // get direct channel
                var channelId = scneario.GetClientManager().GetChannelsStatus().First().Id;
                var channel = scneario.GetClientManager().GetRawChannel(channelId);
                Assert.IsNotNull(channel);

                // make a call
                var client = new Greeter.GreeterClient(channel);
                var rsp = client.SayHello(new HelloRequest { Name = "ItsMe" });
                Assert.IsNotNull(rsp.Message);
            }

        }
    }
}

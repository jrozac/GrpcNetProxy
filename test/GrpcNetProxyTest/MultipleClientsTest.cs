using GrpcNetProxyTest.Apl;
using GrpcNetProxyTest.Setup;
using GrpcNetProxy.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Grpc.Core;
using GrpcNetProxyTest.Scenarios;

namespace GrpcNetProxyTest
{

    /// <summary>
    /// Test multiple clients support
    /// </summary>
    [TestClass]
    public class MultipleClientsTest : TestBase<MultipleClientsScenario>
    {

        /// <summary>
        /// Test multiple clients managers are available
        /// </summary>
        [TestMethod]
        public void TestMultipleClientManagers()
        {

            using (var scenario = Setup())
            {
                // one manager per client
                Assert.AreEqual(scenario.ClientCount, scenario.GetClientsManagers().Count);
                Enumerable.Range(0, scenario.GetClientsManagers().Count).ToList().ForEach(index => {
                    Assert.AreEqual(scenario.GetClientName(index), scenario.GetClientsManagers().ElementAt(index).Name);
                });

                // deactivate first client channels
                scenario.GetClientsManagers()[0].GetChannelsIdsForClient().ToList()
                    .ForEach(ch => scenario.GetClientsManagers()[0].DeactivateChannel(ch));

                // get service  for first client
                var service = scenario.GetClientProvider()
                    .GetGrpcClientService<ITestService>(scenario.GetClientName(0));

                // remote call throw due to unavialable channel (all are disabled)
                var exc = Assert.ThrowsException<RpcException>(() =>
                {
                    service.TestMethodSuccess(GetNewRequest()).GetAwaiter().GetResult();
                });
                Assert.IsTrue(exc.Status.Detail.Contains("channels"));

                // call with another client - it must suceed
                service = scenario.GetClientProvider().GetGrpcClientService<ITestService>(scenario.GetClientName(1));
                var req = GetNewRequest();
                var rsp = service.TestMethodSuccess(req).GetAwaiter().GetResult();
                Assert.AreEqual(req.Id, rsp.Id);
            }

        }

    }
}

using Grpc.Core;
using GrpcNetProxyTest.Apl;
using GrpcNetProxyTest.Scenarios;
using GrpcNetProxyTest.Setup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GrpcNetProxyTest
{

    /// <summary>
    /// Status service test
    /// </summary>
    [TestClass]
    public class StatusServiceTest : TestBase<StatusServiceScenario>
    {

        /// <summary>
        /// Test that failed service status sets application offline status to channel
        /// </summary>
        [TestMethod]
        public void TestThatFailedServiceStatusSetsAplOffline()
        {
            using (var scenario = Setup())
            {
                // verify channel is online
                var chStatus = scenario.GetClientChannelStatus();
                Assert.IsTrue(chStatus.IsAplOnline);

                // server set status to false
                ServerStatusService.Status = false;
                ExecuteServiceStatusCheck(scenario, false);

                // verify channel is offfline
                chStatus = scenario.GetClientChannelStatus();
                Assert.IsFalse(chStatus.IsAplOnline);
            }
        }

        /// <summary>
        /// Test thas success service status sets apl online
        /// </summary>
        [TestMethod]
        public void TestThatSuccessServiceStatusSetsAplOnline()
        {
            using (var scenario = Setup())
            {
                // server set status to false
                ServerStatusService.Status = false;
                ExecuteServiceStatusCheck(scenario, false);

                // verify channel is offfline
                var chStatus = scenario.GetClientChannelStatus();
                Assert.IsFalse(chStatus.IsAplOnline);

                // server set status to true
                ServerStatusService.Status = true;
                ExecuteServiceStatusCheck(scenario, true);

                // verify channel is online
                chStatus = scenario.GetClientChannelStatus();
                Assert.IsTrue(chStatus.IsAplOnline);
            }

        }

        /// <summary>
        /// Test that service status success resets errors
        /// </summary>
        [TestMethod]
        public void TestThatSuccessServiceStatusResetsErrorCounter()
        {

            using (var scenario = Setup())
            {
                // make some bad requests
                Enumerable.Range(0, 2).ToList().ForEach((i) => {
                    var req = GetNewRequest();
                    Assert.ThrowsException<RpcException>(() =>
                    {
                        scenario.GetClientTestService().TestMethodThrow(req).GetAwaiter().GetResult();
                    });
                });

                // verify there are errors
                var chStatus = scenario.GetClientChannelStatus();
                Assert.IsTrue(chStatus.ErrorCount > 0);

                // get online status 
                ExecuteServiceStatusCheck(scenario, true);

                // verify there are no errors
                chStatus = scenario.GetClientChannelStatus();
                Assert.AreEqual(0, chStatus.ErrorCount);
            }
 
        }

        /// <summary>
        /// Execute service status check
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="expectedStatus"></param>
        private void ExecuteServiceStatusCheck(StatusServiceScenario scenario, bool expectedStatus)
        {
            var channelId = scenario.GetClientManager().GetChannelsIdsForClient()[0];
            var status = scenario.GetClientManager().GetChannelRemoteStatusValue(channelId).GetAwaiter().GetResult();
            Assert.AreEqual(expectedStatus, status.Value);
        }

    }
}

using Grpc.Core;
using GrpcNetProxyTest.Scenarios;
using GrpcNetProxyTest.Setup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GrpcNetProxyTest
{

    /// <summary>
    /// Channel status test
    /// </summary>
    [TestClass]
    public class InvokeChannelStatusTest : TestBase<DefaultScenario>
    {

        /// <summary>
        /// Test success request updates invoke counter
        /// </summary>
        [TestMethod]
        public void TestSuccessRequestUpdatesInvokeCounter()
        {
            using (var scenario = Setup())
            {
                // verify channel is online
                var chStatus = scenario.GetClientChannelStatus();
                Assert.IsTrue(chStatus.IsAplOnline);

                // make requst
                var req = GetNewRequest();
                var rsp = scenario.GetClientTestService().TestMethodSuccess(req).GetAwaiter().GetResult();
                Assert.AreEqual(req.Id, rsp.Id);

                // verify invoke counter was incremented
                var chStatusCall = scenario.GetClientChannelStatus();
                Assert.AreEqual(chStatus.InvokeCount + 1, chStatusCall.InvokeCount);
                Assert.AreEqual(chStatus.ErrorCount, chStatusCall.ErrorCount);
            }

        }

        /// <summary>
        /// Test that exception updates invoke and error counter
        /// </summary>
        [TestMethod]
        public void TestRequestThrowUpdatesInvokeCounterAndErrorCounter()
        {

            using (var scenario = Setup())
            {
                // verify channel is online
                var chStatus = scenario.GetClientChannelStatus();
                Assert.IsTrue(chStatus.IsAplOnline);

                // make requst
                var req = GetNewRequest();
                Assert.ThrowsException<RpcException>(() =>
                {
                    scenario.GetClientTestService().TestMethodThrow(req).GetAwaiter().GetResult();
                });

                // verify invoke counter was incremented
                var chStatusCall = scenario.GetClientChannelStatus();
                Assert.AreEqual(chStatus.InvokeCount + 1, chStatusCall.InvokeCount);
                Assert.AreEqual(chStatus.ErrorCount + 1 , chStatusCall.ErrorCount);
            }

        }

        /// <summary>
        /// Test that request timeout updates invoke counter and errror counter
        /// </summary>
        [TestMethod]
        public void TestRequestTimeoutUpdatesInvokeCounterAndErrorCounter()
        {
            using (var scenario = Setup())
            {
                // verify channel is online
                var chStatus = scenario.GetClientChannelStatus();
                Assert.IsTrue(chStatus.IsAplOnline);

                // make requst
                var req = GetNewRequest();
                Assert.ThrowsException<RpcException>(() =>
                {
                    scenario.GetClientTestService().TestMethodTimeout(req).GetAwaiter().GetResult();
                });

                // verify invoke counter was incremented
                var chStatusCall = scenario.GetClientChannelStatus();
                Assert.AreEqual(chStatus.InvokeCount + 1, chStatusCall.InvokeCount);
                Assert.AreEqual(chStatus.ErrorCount + 1, chStatusCall.ErrorCount);
            }

        }

        /// <summary>
        /// Test that success request resets error counter
        /// </summary>
        [TestMethod]
        public void TestThatSuccessRequestResetsErrorCounter()
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

                // check that error count is greater than 0
                var chStatus = scenario.GetClientChannelStatus();
                Assert.IsTrue(chStatus.ErrorCount > 0);

                // make success request 
                var reqs = GetNewRequest();
                var rsp = scenario.GetClientTestService().TestMethodSuccess(reqs).GetAwaiter().GetResult();
                Assert.AreEqual(reqs.Id, rsp.Id);

                // check that error count is greater than 0
                chStatus = scenario.GetClientChannelStatus();
                Assert.AreEqual(0, chStatus.ErrorCount);
            }

        }

    }
}

using Grpc.Core;
using GrpcNetProxyTest.Scenarios;
using GrpcNetProxyTest.Setup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrpcNetProxyTest
{

    /// <summary>
    /// Remove invokation basic test 
    /// </summary>
    [TestClass]
    public class RemoteInvokeTest : TestBase<DefaultScenario>
    {

        /// <summary>
        /// Test method call
        /// </summary>
        [TestMethod]
        public void TestMethodCall()
        {

            using (var scenario = Setup())
            {
                // make call
                var req = GetNewRequest();
                var rsp = scenario.GetClientTestService().TestMethodSuccess(req).GetAwaiter().GetResult();

                // make sure response is valid
                Assert.AreEqual(req.Id, rsp.Id);
            }

        }

        /// <summary>
        /// Test method throw
        /// </summary>
        [TestMethod]
        public void TestMethodThrow()
        {

            using (var scenario = Setup())
            {
                // make call whichs ends up with exception
                var req = GetNewRequest();
                var exc = Assert.ThrowsException<RpcException>(() => {
                    scenario.GetClientTestService().TestMethodThrow(req).GetAwaiter().GetResult();
                });

                // make sure status code is unknown (server exception)
                Assert.AreEqual(StatusCode.Unknown, exc.StatusCode);
            }

        }

        /// <summary>
        /// Test method timeout
        /// </summary>
        [TestMethod]
        public void TestMethodTimeout()
        {

            using (var scenario = Setup())
            {
                var req = GetNewRequest();
                var exc = Assert.ThrowsException<RpcException>(() => {
                    scenario.GetClientTestService().TestMethodTimeout(req).GetAwaiter().GetResult();
                });

                // make sure it was cancelled
                Assert.AreEqual(StatusCode.Cancelled, exc.StatusCode);
            }

        }

    }
}

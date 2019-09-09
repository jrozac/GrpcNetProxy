using Grpc.Core;
using GrpcNetProxyTest.Scenarios;
using GrpcNetProxyTest.Setup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

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

        /// <summary>
        /// Test that each server request is handled in new scope
        /// </summary>
        [TestMethod]
        public void TestEachServerRequestIsHandledInNewScope()
        {
            using (var scenario = Setup())
            {

                // make failed calls (reset counter)
                Assert.ThrowsException<RpcException>(() => {
                    scenario.GetClientTestService().TestMethodThrow(GetNewRequest()).GetAwaiter().GetResult();
                });

                // make 5 calls in sequence
                int seqCall = 5;
                Enumerable.Range(0, seqCall).ToList().ForEach(i => {
                    var req = GetNewRequest();
                    var rsp = scenario.GetClientTestService().TestMethodSuccess(req).GetAwaiter().GetResult();
                    Assert.AreEqual(req.Id, rsp.Id);
                });

                // make 3 calls in parallel
                int pllCall = 3;
                Task.WaitAll(Enumerable.Range(0, pllCall).Select(i => Task.Run(async () =>
                {
                    var req = GetNewRequest();
                    var rsp = await scenario.GetClientTestService().TestMethodSuccess(req);
                    Assert.AreEqual(req.Id, rsp.Id);
                })).ToArray());

                // make last call
                var reqLast = GetNewRequest();
                var rspLast = scenario.GetClientTestService().TestMethodSuccess(reqLast).GetAwaiter().GetResult();
                Assert.AreEqual(reqLast.Id, rspLast.Id);

                // check that constructor on server side was call for every request
                var allCall = seqCall + pllCall + 1;
                Assert.AreEqual(allCall, rspLast.ConstructorInvokeCount);

            }
        }

    }

}

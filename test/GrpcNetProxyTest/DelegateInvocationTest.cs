using GrpcNetProxyTest.Scenarios;
using GrpcNetProxyTest.Setup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GrpcNetProxyTest
{

    /// <summary>
    /// Delegates test
    /// </summary>
    [TestClass]
    public class DelegateInvocationTest : TestBase<DelegatesScenario>
    {

        /// <summary>
        /// Test that context id is propagated through headers from client to server
        /// </summary>
        [TestMethod]
        public void TestContextIdIsPropagatedFromClientToServer()
        {
            using(var scenario = Setup())
            {
                // make 5 calls in sequence
                int seqCall = 5;
                Enumerable.Range(0, seqCall).ToList().ForEach(i => {
                    var req = GetNewRequest();
                    var rsp = scenario.GetClientTestService().TestMethodSuccess(req).GetAwaiter().GetResult();
                    Assert.AreEqual(req.Id, rsp.Id);
                });

                // there must be 5 context ids in server and client list
                Assert.AreEqual(seqCall, scenario.ClientContextList.Count);
                Assert.AreEqual(seqCall, scenario.ServerContextList.Count);

                // make sure ids are the same
                Enumerable.Range(0, seqCall).ToList().ForEach(i => {
                    Assert.AreEqual(scenario.ClientContextList[i], scenario.ServerContextList[i]);
                });
            }

        }

    }
}

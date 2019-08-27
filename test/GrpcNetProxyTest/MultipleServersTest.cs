using GrpcNetProxy.Client;
using GrpcNetProxyTest.Scenarios;
using GrpcNetProxyTest.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcNetProxyTest
{

    /// <summary>
    /// Multiple servers test
    /// </summary>
    [TestClass]
    public class MultipleServersTest : TestBase<MultiServerScenario>
    {

        /// <summary>
        /// Test round robin picks all channels
        /// </summary>
        [TestMethod]
        public void TestRoundRobinPicksAllChannels()
        {
            using(var scenario = Setup())
            {
                // make some loops to execute remote method 
                int loops = 3;
                MakeRequests(scenario,loops);

                // make sure all channels were equally used 
                var chStatuss = scenario.GetClientChannelsStatus();
                chStatuss.ForEach(s => Assert.AreEqual(loops, s.InvokeCount));

            }
        }

        /// <summary>
        /// Test that round robin policy skips deactivated channels
        /// </summary>
        [TestMethod]
        public void TestRoundRobinSkipsDeactivatedChannels()
        {
            using (var scenario = Setup())
            {
                // disable all channels except the first one 
                var activeChId = scenario.GetClientManager().GetChannelsIdsForClient().First();
                scenario.GetClientManager().GetChannelsIdsForClient().Skip(1).ToList()
                    .ForEach(chid => scenario.GetClientManager().DeactivateChannel(chid));

                // make requests 
                MakeRequests(scenario,3);

                // get channels status
                var chStatuss = scenario.GetClientChannelsStatus();

                // deactivated channels do not have invokes
                chStatuss.Where(c => c.Id != activeChId).ToList().ForEach(s => Assert.AreEqual(0, s.InvokeCount));

                // active channel invoked all
                var activeCh = chStatuss.First(c => c.Id == activeChId);
                Assert.AreEqual(3 * scenario.ServersCount, activeCh.InvokeCount);
            }
        }

        /// <summary>
        /// Test that parallel requests are not problematic
        /// </summary>
        [TestMethod]
        public void TestParallelCallsSucceed()
        {
            using (var scenario = Setup())
            {
                // get client manager
                var managers = scenario.GetClientProvider().GetServices<GrpcClientManager>();
                Assert.AreEqual(1, managers.Count());
                var manager = managers.First();

                // test config
                int parallel = 5;
                int allCount = parallel * parallel * scenario.ServersCount;

                // run parallel requests 
                var tasks = Enumerable.Range(0, parallel).Select(i => Task.Run(() => MakeRequests(scenario, parallel)));
                Task.WaitAll(tasks.ToArray());

                // get channels status
                var chStatuss = scenario.GetClientChannelsStatus();

                // verify there were no errors and all invoke counter  is corretly incremented
                Assert.AreEqual(0, chStatuss.Sum(s => s.ErrorCount));
                Assert.AreEqual(allCount, chStatuss.Sum(s => s.InvokeCount));
            }
        }

        /// <summary>
        /// Make requests 
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="loops"></param>
        private void MakeRequests(MultiServerScenario scenario, int loops)
        {
            Enumerable.Range(0, scenario.ServersCount * loops).ToList().ForEach(i => {
                var req = GetNewRequest();
                var rsp = scenario.GetClientTestService().TestMethodSuccess(req).GetAwaiter().GetResult();
                Assert.AreEqual(req.Id, rsp.Id);
            });
        }

    }
}

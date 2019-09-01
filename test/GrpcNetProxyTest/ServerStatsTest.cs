using Grpc.Core;
using GrpcNetProxy.Server;
using GrpcNetProxyTest.Scenarios;
using GrpcNetProxyTest.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using static GrpcNetProxy.Shared.GrpcStats;

namespace GrpcNetProxyTest
{

    /// <summary>
    /// Server stats test
    /// </summary>
    [TestClass]
    public class ServerStatsTest : TestBase<ServerStatsScenario>
    {

        /// <summary>
        /// Test that stats are update
        /// </summary>
        [TestMethod]
        public void TestStatsAreUpdated()
        {
            using (var scenario = Setup())
            {
                // verify that stats are empty
                var stats = GetStats(scenario);
                Assert.AreEqual(0, stats.Count);

                // bad request 
                var req = GetNewRequest();
                var exc = Assert.ThrowsException<RpcException>(() => {
                    scenario.GetClientTestService().TestMethodThrow(req).GetAwaiter().GetResult();
                });
                Assert.AreNotEqual(StatusCode.OK, exc.StatusCode);

                // check stats 
                stats = GetStats(scenario);
                Assert.AreEqual(1, stats.Count);
                Assert.AreEqual(1, stats["ITestService.TestMethodThrow"].ReqCount);
                Assert.AreEqual(1, stats["ITestService.TestMethodThrow"].ErrCount);

                // good request
                req = GetNewRequest();
                var rsp = scenario.GetClientTestService().TestMethodSuccess(req).GetAwaiter().GetResult();
                Assert.IsNotNull(rsp);

                // check stats 
                stats = GetStats(scenario);
                Assert.AreEqual(2, stats.Count);
                Assert.AreEqual(1, stats["ITestService.TestMethodThrow"].ReqCount);
                Assert.AreEqual(1, stats["ITestService.TestMethodThrow"].ReqCount);
                Assert.AreEqual(1, stats["ITestService.TestMethodSuccess"].ReqCount);
                Assert.AreEqual(0, stats["ITestService.TestMethodSuccess"].ErrCount);
            }
        }

        /// <summary>
        /// Gets server stats
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        private Dictionary<string, StatsData> GetStats(ServerStatsScenario scenario)
        {
            var stats = scenario.GetServerHost().Services.GetService<GrpcHost>().GetStats();
            return stats;
        }

    }
}

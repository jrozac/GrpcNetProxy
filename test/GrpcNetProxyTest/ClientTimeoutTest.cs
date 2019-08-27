using GrpcNetProxyTest.Scenarios;
using GrpcNetProxyTest.Setup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GrpcNetProxyTest
{

    /// <summary>
    /// Client timeout test
    /// </summary>
    [TestClass]
    public class ClientTimeoutTest : TestBase<ClientTimeoutScenario>
    {

        /// <summary>
        /// Test that long executing time request does not block other requests.
        /// </summary>
        [TestMethod]
        public void TestLongDurationRequestDoesNotBlockOtherRequests()
        {

            using(var scenario = Setup())
            {

                // make a long going request 
                var timeoutTask = Task.Run(() => {
                    var treq = GetNewRequest();
                    try
                    {
                        scenario.GetClientTestService().TestMethodTimeout(treq).GetAwaiter().GetResult();
                    }
                    catch (Exception)
                    {
                    }
                });
                Task.Delay(100).GetAwaiter().GetResult();

                // make another request
                var timer = Stopwatch.StartNew();
                var req = GetNewRequest();
                var rsp = scenario.GetClientTestService().TestMethodSuccess(req).GetAwaiter().GetResult();
                Assert.AreEqual(req.Id, rsp.Id);
                timer.Stop();
                var duration = timer.ElapsedMilliseconds;

                // wait timeout to complete 
                timer = Stopwatch.StartNew();
                Task.WaitAll(timeoutTask);
                timer.Stop();
                var durationTask = timer.ElapsedMilliseconds;
                Assert.IsTrue(durationTask > 8000);

                // make sure duration was short
                Assert.IsTrue(duration < 1000);
            }

        }

    }
}

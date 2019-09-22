using Grpc.Core;
using GrpcNetProxyTest.Scenarios;
using GrpcNetProxyTest.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using static GrpcNetProxyTest.Setup.TestLoggerProvider;

namespace GrpcNetProxyTest
{

    /// <summary>
    /// Client logging test
    /// </summary>
    [TestClass]
    public class ClientLoggingTest :  TestBase<DefaultScenario>
    {

        /// <summary>
        /// Test method call adds log entry for log
        /// </summary>
        [TestMethod]
        public void TestMethodCallLog()
        {

            using (var scenario = Setup())
            {
                // make call
                var req = GetNewRequest();
                var rsp = scenario.GetClientTestService().TestMethodSuccess(req).GetAwaiter().GetResult();

                // make sure response is valid
                Assert.AreEqual(req.Id, rsp.Id);

                // wait for logs to be written
                Task.Delay(200).Wait();

                // check logs 
                var logs = scenario.ClientProvider.GetService<TestLogSink>().Logs;
                var endLogsCount = logs.Count(l => l.Contains("End for action ITestService/TestMethodSuccess"));
                Assert.IsTrue(endLogsCount == 1);
            }

        }

        /// <summary>
        /// Test that method that throws logs error
        /// </summary>
        [TestMethod]
        public void TestMethodThrowLog()
        {

            using (var scenario = Setup())
            {
                // make call whichs ends up with exception
                var req = GetNewRequest();
                var exc = Assert.ThrowsException<RpcException>(() => {
                    scenario.GetClientTestService().TestMethodThrow(req).GetAwaiter().GetResult();
                });

                // wait for logs to be written
                Task.Delay(200).Wait();

                // check logs 
                var logs = scenario.ClientProvider.GetService<TestLogSink>().Logs;
                var endLogsCount = logs.Count(l => l.Contains("End for failed action ITestService/TestMethodThrow"));
                Assert.IsTrue(endLogsCount == 1);
            }

        }

        /// <summary>
        /// Test that proto generated client logs request
        /// </summary>
        [TestMethod]
        public void TestMethodProtoCallLog()
        {

            using (var scenario = Setup())
            {
                // make call
                var client = scenario.ClientProvider.GetService<Greeter.GreeterClient>();
                client.SayHelloAsync(new HelloRequest { Name = "Test1" });
                var rsp = client.SayHello(new HelloRequest { Name = "Test" });

                // wait for request to complete and logs to be written
                Task.Delay(400).Wait();

                // check logs 
                var logs = scenario.ClientProvider.GetService<TestLogSink>().Logs;
                var endLogsCount = logs.Count(l => l.Contains("GrpcClientRequests:End for action Greeter/SayHello"));
                Assert.IsTrue(endLogsCount == 2);
            }

        }

    }
}

using GrpcNetProxy.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using GrpcNetProxyTest.Apl;
using GrpcNetProxy.Shared;
using Grpc.Core;
using GrpcNetProxyTest.Scenarios;
using System.Threading.Tasks;

namespace GrpcNetProxyTest.Setup
{

    /// <summary>
    /// test base 
    /// </summary>
    public class TestBase<TScenario>
        where TScenario : DefaultScenario, new()
    {

        /// <summary>
        /// Test initialize
        /// </summary>
        protected TScenario Setup()
        {

            // wait a bit before setup
            Task.Delay(50).GetAwaiter().GetResult();

            // create scenario
            var scenario = new TScenario();

            // set server status back to true
            ServerStatusService.Status = true;

            // reset all clients channels 
            scenario.ClientProvider.GetServices<GrpcClientManager>().ToList().ForEach(m => m.ResetChannels());

            // make sure that all channels are reset
            scenario.ClientProvider.GetServices<GrpcClientManager>().ToList().ForEach(manager => {
                var chStatuses = manager.GetChannelsStatus();
                chStatuses.ForEach(ch => {
                    Assert.AreEqual(0, ch.ErrorCount);
                    Assert.AreEqual(true, ch.ErrorsBelowThreshold);
                    Assert.AreEqual(scenario.EnableStatusService ? 1 : 0, ch.InvokeCount);
                    Assert.AreEqual(true, ch.IsActive);
                    Assert.AreEqual(true, ch.IsAplOnline);
                    Assert.IsTrue(ch.State == ChannelState.Idle || ch.State == ChannelState.Ready);
                });
            });

            // wait a bit after setup
            Task.Delay(50).GetAwaiter().GetResult();

            return scenario;
        }

        /// <summary>
        /// Get new request 
        /// </summary>
        /// <returns></returns>
        protected TestRequest GetNewRequest()
        {
            return new TestRequest
            {
                Id = Guid.NewGuid().ToString()
            };
        }

    }
}

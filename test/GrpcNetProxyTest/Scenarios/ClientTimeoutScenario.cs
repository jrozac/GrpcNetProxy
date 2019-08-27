using GrpcNetProxyTest.Setup;
using System.Linq;

namespace GrpcNetProxyTest.Scenarios
{

    /// <summary>
    /// Client timeout scenario
    /// </summary>
    public class ClientTimeoutScenario : DefaultScenario
    {

        /// <summary>
        /// Get clients setups with longer timeout
        /// </summary>
        /// <returns></returns>
        public override ClientSetup[] GetClientsSetups()
        {
            var setups = Enumerable.Range(Port, 1).Select(p =>
            new ClientSetup
            {
                EnableStatus = EnableStatusService,
                Ports = new int[] { p },
                Name = $"GrpcClient_{p}",
                TimeoutMs = 10000
            });
            return setups.ToArray();
        }
    }
}

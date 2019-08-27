using GrpcNetProxyTest.Setup;
using System.Linq;

namespace GrpcNetProxyTest.Scenarios
{

    /// <summary>
    /// Multiple clients scenario
    /// </summary>
    public class MultipleClientsScenario : DefaultScenario
    {

        /// <summary>
        /// Clients ports 
        /// </summary>
        public int ClientCount => 3;

        /// <summary>
        /// Get client name 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetClientName(int index) => $"GrpcClient_{index}";

        /// <summary>
        /// Get clients setups (mutiple clients accesing one server)
        /// </summary>
        /// <returns></returns>
        public override ClientSetup[] GetClientsSetups()
        {
            var setups = Enumerable.Range(0, ClientCount).Select(p => new ClientSetup { EnableStatus = EnableStatusService, Ports = new int[] { Port }, Name = GetClientName(p) });
            return setups.ToArray();
        }
    }
}

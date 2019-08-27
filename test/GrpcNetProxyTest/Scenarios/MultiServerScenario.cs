using GrpcNetProxyTest.Setup;
using System.Linq;

namespace GrpcNetProxyTest.Scenarios
{

    /// <summary>
    /// Multi server scenario
    /// </summary>
    public class MultiServerScenario : DefaultScenario
    {

        /// <summary>
        /// Servers count
        /// </summary>
        public int ServersCount = 3;

        /// <summary>
        /// Get server name 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetServerName(int port) => $"GrpcServer_{port}";

        /// <summary>
        /// Setup multiple servers 
        /// </summary>
        /// <returns></returns>
        public override ServerSetup[] GetServersSetup()
        {
            var setups = Enumerable.Range(Port, ServersCount).
                Select(p => new ServerSetup { EnableStatus = EnableStatusService, Port = p, Name = GetServerName(p) });
            return setups.ToArray();
        }

        /// <summary>
        /// Get clients setup (one client with channels to all servers)
        /// </summary>
        /// <returns></returns>
        public override ClientSetup[] GetClientsSetups()
        {
            var setup = new ClientSetup
            {

                EnableStatus = EnableStatusService,
                Ports = Enumerable.Range(Port, ServersCount).ToArray(),
            };
            return new ClientSetup[] { setup };
        }
    }
}

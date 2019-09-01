using GrpcNetProxy.Client;
using GrpcNetProxy.Shared;
using GrpcNetProxyTest.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GrpcNetProxyTest.Scenarios
{
    /// <summary>
    /// Setup scenario
    /// </summary>
    public class DefaultScenario : IDisposable
    {
        /// <summary>
        /// Constructor with init
        /// </summary>
        public DefaultScenario()
        {
            Init();
        }

        /// <summary>
        /// Server host 
        /// </summary>
        private IHost _host;

        /// <summary>
        /// Port (client and server)
        /// </summary>
        public virtual int Port => 5001;

        /// <summary>
        /// Status service 
        /// </summary>
        public virtual bool EnableStatusService => false;

        /// <summary>
        /// Server stats enabled
        /// </summary>
        public virtual bool EnableServerStats => false;

        /// <summary>
        /// Server host
        /// </summary>
        public IHost GetServerHost() => _host;

        /// <summary>
        /// Client provider
        /// </summary>
        public IServiceProvider ClientProvider;

        /// <summary>
        /// Init 
        /// </summary>
        public void Init()
        {

            // server start 
            var serverSetups = GetServersSetup();
            _host = ServerSetupUtil.CreateHost(serverSetups);
            _host.RunAsync();

            // client
            var clientSetups = GetClientsSetups();
            ClientProvider = ClientSetupUtil.CreateProvider(clientSetups);
            var chStatus = ClientProvider.GetRequiredService<GrpcClientManager>().GetChannelsStatus();
            Assert.IsNotNull(chStatus);
        }

        /// <summary>
        /// Get server setup
        /// </summary>
        /// <returns></returns>
        public virtual ServerSetup[] GetServersSetup()
        {
            var setups = Enumerable.Range(Port, 1).Select(p => new ServerSetup {
                EnableStatus = EnableStatusService,
                Port = p,
                Name = $"GrpcServer_{p}",
                EnableStats = EnableServerStats
            });
            return setups.ToArray();
        }

        /// <summary>
        /// Get clients setups
        /// </summary>
        /// <returns></returns>
        public virtual ClientSetup[] GetClientsSetups()
        {
            var setups = Enumerable.Range(Port, 1).Select(p => new ClientSetup { EnableStatus = EnableStatusService, Ports = new int[] { p }, Name = $"GrpcClient_{p}" });
            return setups.ToArray();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _host.StopAsync();
            _host = null;
        }
    }
}

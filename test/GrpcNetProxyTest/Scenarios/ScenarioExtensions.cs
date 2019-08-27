using GrpcNetProxy.Client;
using GrpcNetProxy.Shared;
using GrpcNetProxyTest.Apl;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrpcNetProxyTest.Scenarios
{

    /// <summary>
    /// Scenario extension methods
    /// </summary>
    public static class ScenarioExtensions
    {

        /// <summary>
        /// Client test services
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public static List<ITestService> GetClientTestServices<TScenario>(this TScenario scenario)
            where TScenario : DefaultScenario
            => scenario.ClientProvider.GetServices<ITestService>().ToList();

        /// <summary>
        /// Clienmt test first available service
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public static ITestService GetClientTestService<TScenario>(this TScenario scenario)
            where TScenario : DefaultScenario
            => GetClientTestServices(scenario).First();

        /// <summary>
        /// Client channels statsu 
        /// </summary>
        /// <param name="scenarios"></param>
        /// <returns></returns>
        public static List<GrpcChannelStatus> GetClientChannelsStatus<TScenario>(this TScenario scenario)
            where TScenario : DefaultScenario
            => scenario.ClientProvider.GetServices<GrpcClientManager>().
                SelectMany(m => m.GetChannelsStatus()).ToList();

        /// <summary>
        /// First client channel status
        /// </summary>
        /// <param name="scenarios"></param>
        /// <returns></returns> 
        public static GrpcChannelStatus GetClientChannelStatus<TScenario>(this TScenario scenario)
            where TScenario : DefaultScenario
            => GetClientChannelsStatus(scenario).First();

        /// <summary>
        /// Get clients manager
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public static List<GrpcClientManager> GetClientsManagers<TScenario>(this TScenario scenario)
            where TScenario : DefaultScenario
            => scenario.ClientProvider.GetServices<GrpcClientManager>().ToList();

        /// <summary>
        /// Get first client manager
        /// </summary>
        /// <param name="scenarios"></param>
        public static GrpcClientManager GetClientManager<TScenario>(this TScenario scenario)
            where TScenario : DefaultScenario
            => GetClientsManagers(scenario).First();

        /// <summary>
        /// Get client provider
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public static IServiceProvider GetClientProvider<TScenario>(this TScenario scenario)
            where TScenario : DefaultScenario 
            => scenario?.ClientProvider;

        /// <summary>
        /// Get server status service
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public static ServerStatusService GetServerStatusService<TScenario>(this TScenario scenario)
            where TScenario : DefaultScenario
            => (ServerStatusService)scenario.GetServerHost().Services.GetService<IStatusService>();

    }
}

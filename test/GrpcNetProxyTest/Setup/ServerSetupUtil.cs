using GrpcNetProxy.Shared;
using GrpcNetProxy.DependencyInjection;
using GrpcNetProxyTest.Apl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GrpcNetProxy.Server;
using System.Linq;

namespace GrpcNetProxyTest.Setup
{

    /// <summary>
    /// Server setup util
    /// </summary>
    public static class ServerSetupUtil
    {

        /// <summary>
        /// Create host 
        /// </summary>
        /// <returns></returns>
        public static IHost CreateHost(params ServerSetup[] setups)
        {
            // host builder
            var serverHostBuilder = new HostBuilder().ConfigureServices((hostContext, services) => {

                // register services
                services.AddScoped<ITestService, ServerTestService>();
                services.AddScoped<IStatusService, ServerStatusService>();

                // setup servers
                setups.ToList().ForEach(setup => {

                    // server setup
                    services.AddGrpcServer(cfg =>
                    {
                        cfg.SetConnection(new GrpcServerConnectionData { Port = setup.Port, Url = "127.0.0.1" });
                        cfg.AddService<ITestService>();
                        if(setup.EnableStatus)
                        {
                            cfg.AddStatusService();
                        }
                        if(!string.IsNullOrWhiteSpace(setup.Name))
                        {
                            cfg.SetName(setup.Name);
                        }
                    });
                    if (!string.IsNullOrWhiteSpace(setup.Name))
                    {
                        services.AddGrpcHostedService(setup.Name);
                    } else
                    {
                        services.AddGrpcHostedService();
                    }
                });

            });

            // build host
            return serverHostBuilder.Build();
        }

    }
}

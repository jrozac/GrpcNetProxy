using GrpcNetProxy.Client;
using GrpcNetProxy.DependencyInjection;
using GrpcNetProxyTest.Apl;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace GrpcNetProxyTest.Setup
{


    /// <summary>
    /// Client setup util
    /// </summary>
    public static class ClientSetupUtil
    {
        /// <summary>
        /// Create provider
        /// </summary>
        /// <param name="setups"></param>
        /// <returns></returns>
        public static IServiceProvider CreateProvider(params ClientSetup[] setups)
        {

            // init collection
            var collection = new ServiceCollection();

            // setup clients
            setups.ToList().ForEach(setup => {

                // add grpc client one
                collection.AddGrpcClient(cfg => {

                    // enable status 
                    if(setup.EnableStatus)
                    {
                        cfg.EnableStatusService();
                    }

                    // set name 
                    if (!string.IsNullOrWhiteSpace(setup.Name))
                    {
                        cfg.SetName(setup.Name);
                    }
                    
                    // add services
                    cfg.AddService<ITestService>();

                    // add channels
                    setup.Ports.ToList().ForEach(port => {
                        cfg.AddHost(new GrpcChannelConnectionData
                        {
                            Port = port,
                            Url = "127.0.0.1"
                        });
                    });

                    // client options 
                    var opts = new GrpcClientOptions
                    {
                        TimeoutMs = setup.TimeoutMs
                    };
                    cfg.SetClientOptions(opts);

                });

            });

            // build provider for services
            return collection.BuildServiceProvider();
        }

    }
}

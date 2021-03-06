﻿using GrpcNetProxy.Client;
using GrpcNetProxy.DependencyInjection;
using GrpcNetProxyTest.Apl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using static GrpcNetProxyTest.Setup.TestLoggerProvider;

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
        public static IServiceProvider CreateProvider(Action<string, ClientConfigurator> customSetup, params ClientSetup[] setups)
        {

            // init collection
            var collection = new ServiceCollection();

            // add logging
            var logSink = new TestLogSink();
            collection.AddSingleton(logSink);
            collection.AddLogging(cfg => cfg.AddProvider(new TestLoggerProvider(logSink)));

            // setup clients
            setups.ToList().ForEach(setup => {

                // add grpc client one
                collection.AddGrpcClient(cfg => {

                    // set name 
                    if (!string.IsNullOrWhiteSpace(setup.Name))
                    {
                        cfg.SetName(setup.Name);
                    }
                    
                    // add services
                    cfg.AddService<ITestService>();
                    cfg.AddService<Greeter.GreeterClient>();

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
                        TimeoutMs = setup.TimeoutMs,
                        StatusServiceEnabled = setup.EnableStatus
                    };
                    cfg.SetClientOptions(opts);

                    // custom setup 
                    customSetup?.Invoke(setup.Name, cfg);

                });

            });

            // build provider for services
            return collection.BuildServiceProvider();
        }

    }
}

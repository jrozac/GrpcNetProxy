using GrpcNetProxy.DependencyInjection;
using GrpcNetProxyTest.Apl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace GrpcNetProxyTestApp
{

    /// <summary>
    /// Test app program
    /// </summary>
    public class Program
    {

        /// <summary>
        /// Main program method
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {

            // create server
            var host = new HostBuilder().ConfigureServices((hostContext, services) =>
            {

                // register services
                services.AddScoped<ITestService, ServerTestService>();

                // configure grpc server
                var srvCfgFilePath = Path.Combine(Directory.GetCurrentDirectory(), "grpcServerOnly.json");
                services.ConfigureGrpc(srvCfgFilePath, (cfg) =>
                {
                    cfg.Server().AddService<ITestService>();
                });

            }).Build();

            // create client
            var clientCfgFilePath = Path.Combine(Directory.GetCurrentDirectory(), "grpcClientOnly.json");
            var clientProvider = new ServiceCollection()
                .ConfigureGrpc(clientCfgFilePath, (cfg) => cfg.Client().AddService<ITestService>())
                .BuildServiceProvider();

            // start host 
            host.Start();

            // call grpc 
            var rsp = clientProvider.GetService<ITestService>().TestMethodSuccess(new TestRequest { Id = "TestId" })
                .GetAwaiter().GetResult();

            // print result
            Console.WriteLine(rsp.Id);

            // stop host
            host.StopAsync();

        }
    }
}

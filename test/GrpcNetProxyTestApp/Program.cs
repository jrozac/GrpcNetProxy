using GrpcNetProxy.DependencyInjection;
using GrpcNetProxy.Server;
using GrpcNetProxyTest;
using GrpcNetProxyTest.Apl;
using GrpcNetProxyTestApp.Apl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using static GrpcNetProxyTest.Greeter;

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
                services.AddScoped<GreeterBase, GreeterService>();

                // configure grpc server
                var srvCfgFilePath = Path.Combine(Directory.GetCurrentDirectory(), "grpcServerOnly.json");
                services.ConfigureGrpc(srvCfgFilePath);

            }).Build();

            // create client
            var clientCfgFilePath = Path.Combine(Directory.GetCurrentDirectory(), "grpcClientOnly.json");
            var clientProvider = new ServiceCollection()
                .ConfigureGrpc(clientCfgFilePath)
                .BuildServiceProvider();

            // start host 
            host.Start();   

            // call grpc 
            var rsp = clientProvider.GetService<ITestService>().TestMethodSuccess(new TestRequest { Id = "TestId" })
                .GetAwaiter().GetResult();

            // call grpc usgin build proto client
            var chId = clientProvider.GetGrpcClientManager().GetChannelsIdsForClient().First();
            var channel = clientProvider.GetGrpcClientManager().GetRawChannel(chId);
            var greeterClient = clientProvider.GetService<GreeterClient>();
            var rsp2 = greeterClient.SayHello(new HelloRequest { Name = "ME" });

            // print result
            Console.WriteLine(rsp.Id);
            Console.WriteLine(rsp2.Message);

            // get stats 
            var stats = host.Services.GetService<GrpcHost>().GetStats();
            stats.ToList().ForEach(stat => {
                Console.WriteLine($"{stat.Key}: req={stat.Value.ReqCount}, err={stat.Value.ErrCount}");
            });

            // stop host
            host.StopAsync();

        }
    }
}

using GrpcNetProxy.DependencyInjection;
using GrpcNetProxy.Server;
using GrpcNetProxySampleShared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace GrpcNetProxySampleServer
{

    /// <summary>
    /// Server program
    /// </summary>
    public static class Program
    {

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var host = CreateHost();
            host.Run();
        }

        /// <summary>
        /// Create host 
        /// </summary>
        /// <returns></returns>
        private static IHost CreateHost ()
        {
            // host builder
            var serverHostBuilder = new HostBuilder().ConfigureServices((hostContext, services) => {

                // register services
                services.AddScoped<IUserService, UserService>();

                // create grpc server one
                services.AddGrpcServer(cfg => 
                    cfg.SetConnection(new GrpcServerConnectionData { Port = 5000, Url = "127.0.0.1" }).
                    AddService<IUserService>().
                    SetName("GrpcServiceOne").
                    SetOnRequestEndAction((l,c, data) => Console.WriteLine($"Request completed at {DateTime.Now}."))
                );
                services.AddGrpcHostedService("GrpcServiceOne");

                // create grpc server two
                services.AddGrpcServer(cfg =>
                    cfg.SetConnection(new GrpcServerConnectionData { Port = 5001, Url = "127.0.0.1" }).
                    SetName("GrpcServiceTwo").
                    AddService<IUserService>().
                    SetOnRequestEndAction((l, c, data) => Console.WriteLine($"Request completed at {DateTime.Now}."))
                );
                services.AddGrpcHostedService("GrpcServiceTwo");

                // add logging 
                services.AddLogging(l => l.AddConsole());
            });

            // build host
            return serverHostBuilder.Build();
        }

    }
}

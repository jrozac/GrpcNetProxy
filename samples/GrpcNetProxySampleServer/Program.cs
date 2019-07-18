using GrpcNetProxy.DependencyInjection;
using GrpcNetProxy.Server;
using GrpcNetProxySampleShared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace GrpcNetProxySampleServer
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHost();
            host.Run();
        }

        private static IHost CreateHost ()
        {
            var serverHostBuilder = new HostBuilder().ConfigureServices((hostContext, services) => {
                services.AddScoped<IUserService, UserService>();
                services.AddGrpcServer(cfg => 
                    cfg.SetConnection(new GrpcServerConnectionData { Port = 5000, Url = "127.0.0.1" }).
                    AddService<IUserService>().
                    SetOnRequestEndAction((l,c, data) => Console.WriteLine($"Request completed at {DateTime.Now}."))
                );
                services.AddGrpcHostedService();
                services.AddLogging(l => l.AddConsole());
            });
            return serverHostBuilder.Build();

        }

    }
}

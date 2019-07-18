using GrpcNetProxy.Client;
using GrpcNetProxy.DependencyInjection;
using GrpcNetProxySampleShared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxySampleClient
{
    static class Program
    {
        static void Main(string[] args)
        {
            var provider = CreateProvider();
            var userService = provider.GetRequiredService<IUserService>();

            var callTasks = Enumerable.Range(0, 10).
                Select(i => userService.GetUser(new UserFilter { Id = i.ToString() }, new CancellationToken())).ToArray();

            Task.WaitAll(callTasks);
            Console.ReadLine();

        }

        static IServiceProvider CreateProvider()
        {
            var collection = new ServiceCollection();
            collection.AddGrpcClient<IUserService>(cfg => cfg.AddHost(new GrpcChannelConnectionData
            {
                Port = 5000,
                Url = "127.0.0.1"
            }));
            collection.AddLogging(cfg => cfg.AddConsole());
            return collection.BuildServiceProvider(); 
        }
    }
}

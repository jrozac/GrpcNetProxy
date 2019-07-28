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

    /// <summary>
    /// Client
    /// </summary>
    public static class Program
    {

        /// <summary>
        /// Client main method
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // create service provider 
            var provider = CreateProvider();

            // get service 
            var userServiceOne = provider.GetGrpcClientService<IUserService>("GrpcClientOne");
            var userServiceTwo = provider.GetGrpcClientService<IUserService>("GrpcClientTwo");

            // run methods on both services
            var callTasks = Enumerable.Range(0, 10).
                Select(i => (i % 2 == 0 ? userServiceOne : userServiceTwo).GetUser(new UserFilter { Id = i.ToString() }, new CancellationToken())).ToArray();
            Task.WaitAll(callTasks);

            // get new line
            Console.ReadLine();

        }

        /// <summary>
        /// Create service provider
        /// </summary>
        /// <returns></returns>
        static IServiceProvider CreateProvider()
        {
            // init collection
            var collection = new ServiceCollection();

            // add grpc client one
            collection.AddGrpcClient(cfg => cfg.AddHost(new GrpcChannelConnectionData
            {
                Port = 5000,
                Url = "127.0.0.1"
            }).AddService<IUserService>().
            SetName("GrpcClientOne"));

            // add grpc client two
            collection.AddGrpcClient(cfg => cfg.AddHost(new GrpcChannelConnectionData
            {
                Port = 5001,
                Url = "127.0.0.1"
            }).AddService<IUserService>().
            SetName("GrpcClientTwo"));

            // add logging and build provider for services
            collection.AddLogging(cfg => cfg.AddConsole());
            return collection.BuildServiceProvider(); 
        }
    }
}

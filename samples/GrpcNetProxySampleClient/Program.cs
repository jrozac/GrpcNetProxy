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

            // get managers
            var managerOne = provider.GetGrpcClientManager("GrpcClientOne");
            var managerTwo = provider.GetGrpcClientManager("GrpcClientTwo");

            // run methods on both services
            int count = 16;
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var callTasks = Enumerable.Range(0, count).
                Select(i => (i % 2 == 0 ? userServiceOne : userServiceTwo).GetUser(new UserFilter { Id = i.ToString() }, token)).ToArray();
            Task.WaitAll(callTasks);

            // print results 
            callTasks.ToList().ForEach(t => Console.WriteLine($"Got user {t.Result.Id} names {t.Result.Name}."));

            // get status
            var chStatusOne = managerOne.GetChannelsStatus();
            var chStatusTwo = managerTwo.GetChannelsStatus();

            // print statuses 
            chStatusOne.ForEach(s => Console.WriteLine($"Status client one channel: {s.Id}, invoke count: {s.InvokeCount}."));
            chStatusTwo.ForEach(s => Console.WriteLine($"Status client two channel: {s.Id}, invoke count: {s.InvokeCount}."));

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
            }).AddHost(new GrpcChannelConnectionData {
                Port = 5001,
                Url = "127.0.0.1"
            }).
            AddService<IUserService>().
            EnableStatusService().
            SetName("GrpcClientOne"));

            // add grpc client two
            collection.AddGrpcClient(cfg => cfg.AddHost(new GrpcChannelConnectionData
            {
                Port = 5001,
                Url = "127.0.0.1"
            }).AddService<IUserService>().
            EnableStatusService().
            SetName("GrpcClientTwo"));

            // add logging and build provider for services
            collection.AddLogging(cfg => cfg.AddConsole());
            return collection.BuildServiceProvider(); 
        }
    }
}

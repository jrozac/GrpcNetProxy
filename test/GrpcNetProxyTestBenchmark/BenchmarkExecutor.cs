using GrpcNetProxyTest.Apl;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GrpcNetProxyTest.Setup;

namespace GrpcNetProxyTestBenchmark
{

    /// <summary>
    /// Becnhmark executor
    /// </summary>
    public class BenchmarkExecutor
    {

        /// <summary>
        /// Port start
        /// </summary>
        private int _port = 5000;

        /// <summary>
        /// Server count
        /// Server count
        /// </summary>
        private int _serverCount = 2;

        /// <summary>
        /// Host
        /// </summary>
        private IHost _host;

        /// <summary>
        /// Client services
        /// </summary>
        private IServiceProvider _clientServices;

        /// <summary>
        /// Request count
        /// </summary>
        private long _reqCount;

        /// <summary>
        /// Error count
        /// </summary>
        private long _errCount;

        /// <summary>
        /// Stopwatch for global purposes
        /// </summary>
        private Stopwatch _globalWatch;

        /// <summary>
        /// set log interval
        /// </summary>
        private int _logInterval = 1000;

        /// <summary>
        /// Parallelism level
        /// </summary>
        private int _parallelismLevel;

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="tkn"></param>
        public void Run(CancellationToken tkn, int parallelismLevel = 1)
        {

            // set parallelism level
            _parallelismLevel = parallelismLevel;

            // setup
            SetupHost();
            SetupClients();
            _globalWatch = Stopwatch.StartNew();

            // run in tasks
            var runTasks = Enumerable.Range(0, _parallelismLevel)
                .ToList().Select((i) => Task.Run(async() => await RunBenchmark(tkn, i * 500)))
                .ToArray();
            Task.WaitAll(runTasks);
        }

        /// <summary>
        /// Run benchmark
        /// </summary>
        /// <param name="tkn"></param>
        /// <param name="delayMs"></param>
        private async Task RunBenchmark(CancellationToken tkn, int delayMs)
        {
            // wait
            await Task.Delay(delayMs);

            // run
            while (!tkn.IsCancellationRequested)
            {

                // wait a bit amog requests
                await Task.Delay(20);

                // get id 
                long id = Interlocked.Increment(ref _reqCount);

                // make request
                var testSvc = _clientServices.GetService<ITestService>();
                try
                {
                    var rsp = await testSvc.TestMethodSuccess(new TestRequest
                    {
                        Id = id.ToString()
                    });
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref _errCount);
                }

                // log 
                if (id % _logInterval == 0)
                {
                    var avg = _globalWatch.ElapsedMilliseconds / _reqCount;
                    Console.WriteLine($"reqCoutn: {_reqCount}, errCount: {_errCount}, watchMs: {_globalWatch.ElapsedMilliseconds}, avgReqMs: {avg}");
                }
            }
        }

        /// <summary>
        /// Setup hosts 
        /// </summary>
        private void SetupHost()
        {
            var serverSetups = Enumerable.Range(0, _serverCount)
                .Select(i => new ServerSetup
                {
                    EnableStatus = true,
                    Name = $"Server_{_port + i}",
                    Port = _port + i
                }).ToArray();
            _host = ServerSetupUtil.CreateHost(serverSetups);
            _host.RunAsync();
        }

        /// <summary>
        /// Setup clients
        /// </summary>
        private void SetupClients()
        {
            var clientSetup = new ClientSetup
            {
                EnableStatus = true,
                Name = "Default client",
                TimeoutMs = 1000,
                Ports = Enumerable.Range(0, _serverCount).Select(i => _port + i).ToArray()
            };
            _clientServices = ClientSetupUtil.CreateProvider(clientSetup);

        }

    }
}

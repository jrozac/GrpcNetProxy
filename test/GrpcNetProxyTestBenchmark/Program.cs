using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxyTestBenchmark
{

    /// <summary>
    /// Program
    /// </summary>
    class Program
    {

        /// <summary>
        /// Paralellism level
        /// </summary>
        private static int _parallelismLevel = 10;

        /// <summary>
        /// Main run
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            // cancelation token
            var ctkn = new CancellationTokenSource();

            // run execution
            var executor = new BenchmarkExecutor();
            var execTask = Task.Run(() =>
            {
                executor.Run(ctkn.Token, _parallelismLevel);
            });

            // wait for cancel                
            var cancelTask = Task.Run(() =>
            {
                Console.WriteLine("Running becnhmark. Press enter to stop.");
                Console.ReadLine();
                ctkn.Cancel();
            });

            // wait any task to complete
            Task.WaitAny(execTask);
            
        }

    }
}

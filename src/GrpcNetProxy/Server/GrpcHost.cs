using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GrpcNetProxy.Server
{

    /// <summary>
    /// Grpc host
    /// </summary>
    public class GrpcHost
    {

        /// <summary>
        /// Prevent external creations with internal constructor
        /// </summary>
        internal GrpcHost() { }

        /// <summary>
        /// Configuration
        /// </summary>
        private readonly GrpcServerConfiguration _cfg;

        /// <summary>
        /// Service provider
        /// </summary>
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Host server
        /// </summary>
        private Grpc.Core.Server _server;

        /// <summary>
        /// Constructor with server and configuration as parameters
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cfg"></param>
        internal GrpcHost(IServiceProvider provider, GrpcServerConfiguration cfg)
        {
            _cfg = cfg;
            _provider = provider;
        }

        /// <summary>
        /// Init server (must be synchronized)
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Init()
        {
            // shutdown if already active 
            _server?.ShutdownAsync().GetAwaiter().GetResult();

            // create server again
            _server = GrpcServerBuilder.Build(_provider, _cfg);
        }

        /// <summary>
        /// Start server
        /// </summary>
        /// <returns></returns>
        public Task StartAsync()
        {
            Init();
            _server.Start();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop server
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            await _server.ShutdownAsync().ConfigureAwait(false);
        }

    }
}

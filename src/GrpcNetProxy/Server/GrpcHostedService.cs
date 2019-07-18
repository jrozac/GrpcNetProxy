using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxy.Server
{

    /// <summary>
    /// Hosted service grpc
    /// </summary>
    public class GrpcHostedService : IHostedService
    {
        /// <summary>
        /// Host
        /// </summary>
        private readonly GrpcHost _host;

        /// <summary>
        /// Constructor with injected host
        /// </summary>
        /// <param name="host"></param>
        internal GrpcHostedService(GrpcHost host)
        {
            _host = host;
        }

        /// <summary>
        /// Start host
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _host.StartAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Stop host
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _host.StopAsync().ConfigureAwait(false);
        }

    }
}

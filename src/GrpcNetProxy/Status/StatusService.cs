using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxy.Status
{
    /// <summary>
    /// Service status implementatiton
    /// </summary>
    public class StatusService : IStatusService
    {
        /// <summary>
        /// Check service status
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<CheckStatusResponse> CheckStatus(CheckStatusRequest request, CancellationToken token)
        {
            return Task.FromResult(new CheckStatusResponse { Status = true });
        }
    }
}

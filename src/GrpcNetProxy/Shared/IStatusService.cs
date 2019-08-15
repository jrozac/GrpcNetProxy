using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxy.Shared
{

    /// <summary>
    /// Status service. Used to check server status from clients.
    /// </summary>
    public interface IStatusService
    {

        /// <summary>
        /// Check service status
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<CheckStatusResponse> CheckStatus(CheckStatusRequest request, CancellationToken token);

    }
}

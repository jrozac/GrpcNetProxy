using GrpcNetProxy.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxyTest.Apl
{

    /// <summary>
    /// Server status service
    /// </summary>
    public class ServerStatusService : IStatusService
    {

        /// <summary>
        /// Status value for test 
        /// </summary>
        public bool Status { get; set; } = true;

        /// <summary>
        /// Check status 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<CheckStatusResponse> CheckStatus(CheckStatusRequest request, CancellationToken token = default)
        {
            return Task.FromResult(new CheckStatusResponse { Status = Status });
        }
    }
}

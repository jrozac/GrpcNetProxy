using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxyTest.Apl
{

    /// <summary>
    /// Test service server implementation
    /// </summary>
    public class ServerTestService : ITestService
    {

        /// <summary>
        /// Create constructor counter
        /// </summary>
        private static int _createCount;

        /// <summary>
        /// Constructor increments create count 
        /// </summary>
        public ServerTestService()
        {
            Interlocked.Increment(ref _createCount);
        }

        /// <summary>
        /// Method success
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<TestResponse> TestMethodSuccess(TestRequest request, CancellationToken token = default)
        {
            return Task.FromResult(new TestResponse { Id = request.Id, ConstructorInvokeCount = _createCount });
        }

        /// <summary>
        /// Method throw
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<TestResponse> TestMethodThrow(TestRequest request, CancellationToken token = default)
        {
            Interlocked.Exchange(ref _createCount, 0);
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Timeout method
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<TestResponse> TestMethodTimeout(TestRequest request, CancellationToken token = default)
        {
            Interlocked.Exchange(ref _createCount, 0);
            Task.Delay(5 * 60 * 1000, token).GetAwaiter().GetResult();
            return Task.FromResult<TestResponse>(null);
        }
    }
}

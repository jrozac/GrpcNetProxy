using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxyTest.Apl
{

    /// <summary>
    /// Test service
    /// </summary>
    public interface ITestService
    {

        /// <summary>
        /// Test successs
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<TestResponse> TestMethodSuccess(TestRequest request, CancellationToken token = default);

        /// <summary>
        /// Test method throw
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<TestResponse> TestMethodThrow(TestRequest request, CancellationToken token = default);

        /// <summary>
        /// Test method timeout
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<TestResponse> TestMethodTimeout(TestRequest request, CancellationToken token = default);

    }
}

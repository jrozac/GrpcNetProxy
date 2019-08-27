﻿using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxyTest.Apl
{

    /// <summary>
    /// Test service server implementation
    /// </summary>
    public class ServerTestService : ITestService
    {
        /// <summary>
        /// Method success
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<TestResponse> TestMethodSuccess(TestRequest request, CancellationToken token = default)
        {
            return Task.FromResult(new TestResponse { Id = request.Id });
        }

        /// <summary>
        /// Method throw
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<TestResponse> TestMethodThrow(TestRequest request, CancellationToken token = default)
        {
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
            Task.Delay(5 * 60 * 1000, token).GetAwaiter().GetResult();
            return Task.FromResult<TestResponse>(null);
        }
    }
}

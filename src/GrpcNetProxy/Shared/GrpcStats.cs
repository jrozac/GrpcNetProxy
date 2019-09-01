using Grpc.Core;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GrpcNetProxy.Shared
{

    /// <summary>
    /// Grpc stats
    /// </summary>
    public class GrpcStats
    {

        /// <summary>
        /// Internal constructor prevers external instances creation
        /// </summary>
        internal GrpcStats() { }

        /// <summary>
        /// Stats data 
        /// </summary>
        public class StatsData
        {
            internal StatsData() { }
            private long _reqCount;
            private long _errCount;
            public long ReqCount => _reqCount;
            public long ErrCount => _errCount;
            internal void AddReq()
            {
                Interlocked.Increment(ref _reqCount);
            }
            internal void AddError()
            {
                Interlocked.Increment(ref _errCount);
            }
        }

        /// <summary>
        /// stats 
        /// </summary>
        private ConcurrentDictionary<string, StatsData> _stats = new ConcurrentDictionary<string, StatsData>();

        /// <summary>
        /// Reset stats 
        /// </summary>
        public void Reset()
        {
            _stats = new ConcurrentDictionary<string, StatsData>();
        }

        /// <summary>
        /// New request 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="context"></param>
        internal void NewRequest(string serviceName, string methodName, ServerCallContext context)
        {
            GetAction(serviceName, methodName).AddReq();
        }

        /// <summary>
        /// New error
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="context"></param>
        /// <param name="errMsg"></param>
        internal void RequestError(string serviceName, string methodName, ServerCallContext context, string errMsg)
        {
            GetAction(serviceName, methodName).AddError();
        }

        /// <summary>
        /// Get stats 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, StatsData> GetStats()
        {
            return _stats.ToDictionary(k => k.Key, v => v.Value);
        }

        /// <summary>
        /// Get data 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private StatsData GetAction(string serviceName, string methodName)
        {
            string name = $"{serviceName}.{methodName}";
            return _stats.GetOrAdd(name, (n) => new StatsData());
        }

    }
}

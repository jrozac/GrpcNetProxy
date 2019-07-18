using System;

namespace GrpcNetProxy.Shared
{
    /// <summary>
    /// Request end data
    /// </summary>
    public class RequestEndData : RequestStartData
    {
        /// <summary>
        /// Call druation
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Response object
        /// </summary>
        public object Response { get; set; }

        /// <summary>
        /// Exception
        /// </summary>
        public Exception Exception { get; set; }
    }
}

using GrpcNetProxy.Shared;
using Microsoft.Extensions.Logging;
using System;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Client data handlers
    /// </summary>
    public class GrpcClientDataHandlers
    {

        /// <summary>
        /// On request start 
        /// </summary>
        public Action<ILogger, RequestStartData> OnRequestStart { get; set; }

        /// <summary>
        /// On request end
        /// </summary>
        public Action<ILogger, RequestEndData> OnRequestEnd { get; set; }

        /// <summary>
        /// Context data getter
        /// </summary>
        public Func<string> ContextGetter { get; set; }

    }
}

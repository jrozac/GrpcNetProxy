using GrpcNetProxy.Shared;
using Microsoft.Extensions.Logging;
using System;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Grpc client configuration
    /// </summary>
    public class GrpcClientConfiguration
    {

        /// <summary>
        /// Internal constructor prevers external creation
        /// </summary>
        internal GrpcClientConfiguration() { }

        /// <summary>
        /// Client options 
        /// </summary>
        public GrpcClientOptions ClientOptions { get; set; } = new GrpcClientOptions();

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

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

    }
}

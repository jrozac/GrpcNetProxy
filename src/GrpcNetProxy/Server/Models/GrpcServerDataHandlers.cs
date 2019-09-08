using Grpc.Core;
using GrpcNetProxy.Shared;
using Microsoft.Extensions.Logging;
using System;

namespace GrpcNetProxy.Server
{

    /// <summary>
    /// Data handlers
    /// </summary>
    internal class GrpcServerDataHandlers
    {

        /// <summary>
        /// On request start acttion
        /// </summary>
        public Action<ILogger, ServerCallContext, RequestStartData> OnRequestStart { get; set; }

        /// <summary>
        /// On request end action
        /// </summary>
        public Action<ILogger, ServerCallContext, RequestEndData> OnRequestEnd { get; set; }

        /// <summary>
        /// Context data setter
        /// </summary>
        public Action<string> ContextSetter { get; set; }

    }
}

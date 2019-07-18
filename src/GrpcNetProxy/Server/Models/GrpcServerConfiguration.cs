using Grpc.Core;
using GrpcNetProxy.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace GrpcNetProxy.Server
{

    /// <summary>
    /// Grpc server configuration
    /// </summary>
    internal class GrpcServerConfiguration
    {

        /// <summary>
        /// Options
        /// </summary>
        public GrpcServerOptions Options { get; set; } = new GrpcServerOptions();

        /// <summary>
        /// Connection
        /// </summary>
        public GrpcServerConnectionData Connection { get; set; } = new GrpcServerConnectionData { Port = 5000, Url = "127.0.0.1" };

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

        /// <summary>
        /// Services interfaces types
        /// </summary>
        public List<Type> ServicesTypes { get; set; } = new List<Type>();

    }
}

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
        /// Configuration name. Important in case of multiple servers.
        /// </summary>
        public string Name { get; set; } = "Default";

        /// <summary>
        /// Data handlers
        /// </summary>
        internal GrpcServerDataHandlers DataHandlers { get; } = new GrpcServerDataHandlers();

        /// <summary>
        /// Options
        /// </summary>
        public GrpcServerOptions Options { get; set; } = new GrpcServerOptions();

        /// <summary>
        /// Connection
        /// </summary>
        public GrpcServerConnectionData Connection { get; set; } = new GrpcServerConnectionData { Port = 5000, Url = "127.0.0.1" };

        /// <summary>
        /// Services interfaces types
        /// </summary>
        public List<Type> ServicesTypes { get; set; } = new List<Type>();

    }
}

using GrpcNetProxy.Server.Models;
using GrpcNetProxy.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static GrpcNetProxy.Shared.GrpcStats;

namespace GrpcNetProxy.Server
{

    /// <summary>
    /// Grpc host
    /// </summary>
    public class GrpcHost
    {

        /// <summary>
        /// Prevent external creations with internal constructor
        /// </summary>
        internal GrpcHost() { }

        /// <summary>
        /// Requests stats
        /// </summary>
        internal GrpcStats Stats { get; }

        /// <summary>
        /// Configuration
        /// </summary>
        private readonly GrpcServerConfiguration _cfg;

        /// <summary>
        /// Service provider
        /// </summary>
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Host server
        /// </summary>
        private Grpc.Core.Server _server;

        /// <summary>
        /// Get host name
        /// </summary>
        public string Name => _cfg?.Name;

        /// <summary>
        /// Stats
        /// </summary>
        public Dictionary<string, StatsData> GetStats() => Stats?.GetStats();

        /// <summary>
        /// Reset stats
        /// </summary>
        public void ResetStats() => Stats?.Reset();

        /// <summary>
        /// Constructor with server and configuration as parameters
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cfg"></param>
        internal GrpcHost(IServiceProvider provider, GrpcServerConfiguration cfg)
        {
            _cfg = cfg;
            _provider = provider;
            if(cfg.Options.StatsEnabled)
            {
                Stats = new GrpcStats();
            }
        }

        /// <summary>
        /// Init server (must be synchronized)
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void Init()
        {
            // shutdown if already active 
            _server?.ShutdownAsync().GetAwaiter().GetResult();

            // create server again
            _server = GrpcServerBuilder.Build(_provider, _cfg);
        }

        /// <summary>
        /// Start server
        /// </summary>
        /// <returns></returns>
        public Task StartAsync()
        {
            Init();
            _server.Start();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop server
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            await _server.ShutdownAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get server info
        /// </summary>
        /// <returns></returns>
        public GrpcServerInfo GetInfo()
        {
            var info = new GrpcServerInfo
            {
                Name = _cfg?.Name,
                Services = _cfg?.ServicesTypes.Select(t => t.Name).ToList(),
                Connections = _server.Ports.Select(p => new GrpcServerInfo.ConnectionInfo {
                    Host = p.Host,
                    Port = p.BoundPort
                }).ToList()
            };
            return info;
        }

    }
}

using Grpc.Core;
using System.Collections.Generic;
using System.Linq;

namespace GrpcNetProxy.Client
{
    /// <summary>
    /// Grpc channels manager
    /// </summary>
    public abstract class GrpcChannelManager
    {

        /// <summary>
        /// Channels options
        /// </summary>
        private readonly GrpcChannelManagerConfiguration _configuration;

        /// <summary>
        /// Channels
        /// </summary>
        private readonly List<Channel> _channels = new List<Channel>();

        /// <summary>
        /// Grpc invokers 
        /// </summary>
        private readonly List<DefaultCallInvoker> _invokers = new List<DefaultCallInvoker>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hosts"></param>
        internal GrpcChannelManager(GrpcChannelManagerConfiguration configuration)
        {
            _configuration = configuration;
            Init();
        }

        /// <summary>
        /// Get next invoker
        /// </summary>
        /// <returns></returns>
        internal CallInvoker NextInvoker()
        {
            // todo: implement round robin or similar
            return _invokers.FirstOrDefault();
        }

        /// <summary>
        /// Init channels
        /// </summary>
        private void Init()
        {
            _channels.AddRange(_configuration.ChannelsOptions.Select(options => new Channel(options.Url, options.Port, ChannelCredentials.Insecure)));
            _invokers.AddRange(_channels.Select(channel => new DefaultCallInvoker(channel)));
        }

        /// <summary>
        /// Get channels status
        /// </summary>
        /// <returns></returns>
        public List<GrpcChannelStatus> GetChannelsStatus()
        {
            return Enumerable.Range(0, _channels.Count).Select(i => new GrpcChannelStatus {
                Options = _configuration.ChannelsOptions[i],
                State = _channels[i].State
            }).ToList();
        }
    }

    /// <summary>
    /// Service type manager
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public class GrpcChannelManager<TService> : GrpcChannelManager
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        internal GrpcChannelManager(GrpcChannelManagerConfiguration configuration) : base(configuration)
        {
        }
    }
}

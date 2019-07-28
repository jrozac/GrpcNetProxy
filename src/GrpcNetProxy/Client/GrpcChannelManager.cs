using Grpc.Core;
using System.Collections.Generic;
using System.Linq;

namespace GrpcNetProxy.Client
{
    /// <summary>
    /// Grpc channels manager
    /// </summary>
    public class GrpcChannelManager
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
        /// Manager name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="configuration"></param>
        internal GrpcChannelManager(string name, GrpcChannelManagerConfiguration configuration)
        {
            Name = name;
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
}

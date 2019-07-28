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
        /// Round robin policy
        /// </summary>
        private RoundRobinPolicy<InvokerBundle> _roundRobin;

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
            return _roundRobin.GetNext().Invoker;
        }

        /// <summary>
        /// Active client condition
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool ActiveClientCondition(InvokerBundle item)
        {
            return item.Channel.State == ChannelState.Idle || item.Channel.State == ChannelState.Ready;
        }

        /// <summary>
        /// Init channels
        /// </summary>
        private void Init()
        {

            // create channels
            var items = _configuration.ChannelsOptions.Select(options => {
                var ch = new Channel(options.Url, options.Port, ChannelCredentials.Insecure);
                var inv = new DefaultCallInvoker(ch);
                return new InvokerBundle(ch, inv, options);
            }).ToList();

            // create invokers
            _roundRobin = new RoundRobinPolicy<InvokerBundle>(items, ActiveClientCondition);

        }

        /// <summary>
        /// Get channels status
        /// </summary>
        /// <returns></returns>
        public List<GrpcChannelStatus> GetChannelsStatus()
        {
            return _roundRobin.GetItems().Select(b => new GrpcChannelStatus {
                Options = b.ConnectionData,
                State = b.Channel.State
            }).ToList();
        }
    }
}

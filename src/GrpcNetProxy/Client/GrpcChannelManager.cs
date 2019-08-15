using Grpc.Core;
using System;
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
        /// Get next invoker bundle
        /// </summary>
        /// <returns></returns>
        internal InvokerBundle NextInvoker()
        {
            return _roundRobin.GetNext();
        }

        /// <summary>
        /// Get score for invoker
        /// </summary>
        /// <param name="ib"></param>
        /// <returns></returns>
        private int GetScore(InvokerBundle ib)
        {

            // not active 
            if (!ib.IsActive)
            {
                return int.MinValue;
            }

            // not connected physically
            if(ib.Channel.State != ChannelState.Idle && ib.Channel.State != ChannelState.Ready)
            {
                return int.MinValue;
            }

            // score offset pow
            int offset = 32768;

            // default score
            int score = 0;

            // apl online (16 points)
            if (ib.IsAplOnline)
            {
                score = score + offset * 16;
            }

            // error under limit (8 points)
            if(ib.ErrorsBelowThreshold)
            {
                score = score + offset * 8;
            }

            // return score
            return score;
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
                var bundle = new InvokerBundle(ch, inv, options);
                ResetChannel(bundle);
                return bundle;
            }).ToList();

            // create invokers
            _roundRobin = new RoundRobinPolicy<InvokerBundle>(items, GetScore);

        }

        /// <summary>
        /// Get channel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal InvokerBundle GetChannel(string id)
        {
            var ch = _roundRobin.GetItems().FirstOrDefault(b => b.Id == id);
            if (ch == null)
            {
                throw new ArgumentException("Channel id is invalid");
            }
            return ch;
        }

        /// <summary>
        /// Gets channels
        /// </summary>
        /// <returns></returns>
        internal List<InvokerBundle> GetChannels()
        {
            return _roundRobin.GetItems();
        }

        /// <summary>
        /// Reset channel
        /// </summary>
        /// <param name="id"></param>
        internal void ResetChannel(string id)
        {
            var ch = GetChannel(id);
            ResetChannel(ch);
        }

        /// <summary>
        /// Reset channel
        /// </summary>
        /// <param name="ch"></param>
        internal void ResetChannel(InvokerBundle ch)
        {

            // set aplication online depending of configuration
            if (_configuration.StatusServiceEnabled)
            {
                ch.SetAplOffline();
            }
            else
            {
                ch.SetAplOnline();
            }

            // reset error and activate channel
            ch.ResetError();
            ch.ResetInvokeCount();
            ch.Activate();
        }

        /// <summary>
        /// Configuration getter
        /// </summary>
        internal GrpcChannelManagerConfiguration Configuration => _configuration;

        /// <summary>
        /// Deactivate channel
        /// </summary>
        /// <param name="id"></param>
        internal void DeactivateChannel(string id)
        {
            var ch = GetChannel(id);
            ch.Deactivate();
        }

        /// <summary>
        /// Activate channel
        /// </summary>
        /// <param name="id"></param>
        internal void ActivateChannel(string id)
        {
            var ch = GetChannel(id);
            ch.Activate();
        }

        /// <summary>
        /// Get channels ids
        /// </summary>
        /// <returns></returns>
        internal string[] GetChannelsIds()
        {
            return _roundRobin.GetItems().Select(i => i.Id).ToArray();
        }
    }
}

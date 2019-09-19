using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Round robin select policy
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    internal class RoundRobinPolicy<TItem>
    {

        /// <summary>
        /// All items
        /// </summary>
        private readonly List<TItem> _items;

        /// <summary>
        /// Condition checker
        /// </summary>
        private readonly Func<TItem, int> _channelScoreDelegate;

        /// <summary>
        /// Current position
        /// </summary>
        private long _current = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items"></param>
        /// <param name="channelScoreDelegate"></param>
        public RoundRobinPolicy(List<TItem> items, Func<TItem, int> channelScoreDelegate)
        {
            _items = items;
            _channelScoreDelegate = channelScoreDelegate;
        }

        /// <summary>
        /// All items getter
        /// </summary>
        /// <returns></returns>
        public List<TItem> GetItems() => _items;

        /// <summary>
        /// Get next item
        /// </summary>
        /// <returns></returns>
        public TItem GetNext()
        {

            // fix overflow
            if (_current > long.MaxValue - 100000)
            {
                Interlocked.Exchange(ref _current, -1);
            }

            // get next position
            var roundRobinPos = Interlocked.Increment(ref _current) % _items.Count;

            // get channel (conditional round-robin)
            var channel = Enumerable.Range(0, _items.Count).
                Select(k => new Tuple<int, TItem>(_channelScoreDelegate(_items.ElementAt(k)) + (roundRobinPos == k ? 1 : 0), _items.ElementAt(k))).
                Where(t => t.Item1 >= 0).OrderByDescending(t => t.Item1).FirstOrDefault();

            // no channel availalbe
            if(channel == null) {
                throw new RpcException(new Grpc.Core.Status(StatusCode.Unavailable, "No channels available."));
            }

            // return
            return channel.Item2;
 
        }

    }
}

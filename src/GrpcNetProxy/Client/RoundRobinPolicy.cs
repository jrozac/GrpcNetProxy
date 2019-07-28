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
        private readonly Func<TItem, bool> _condition;

        /// <summary>
        /// Current position
        /// </summary>
        private long _current = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items"></param>
        /// <param name="conditionDelegate"></param>
        public RoundRobinPolicy(List<TItem> items, Func<TItem, bool> conditionDelegate)
        {
            _items = items;
            _condition = conditionDelegate;
        }

        /// <summary>
        /// All items getter
        /// </summary>
        /// <returns></returns>
        public List<TItem> GetItems() => _items;

        /// <summary>
        /// Get active items
        /// </summary>
        /// <returns></returns>
        public List<TItem> GetActiveItems() => _items.Where(itm => _condition(itm)).ToList();

        /// <summary>
        /// Get next item
        /// </summary>
        /// <returns></returns>
        public TItem GetNext()
        {
            // get active items and return first if none active
            var allItems = GetActiveItems();
            if(!allItems.Any())
            {
                return _items.FirstOrDefault();
            }

            // fix overflow
            if(_current > long.MaxValue - 100000)
            {
                Interlocked.Exchange(ref _current, -1);
            }

            // get next position
            var val = Interlocked.Increment(ref _current);
            var pos = (int) val % allItems.Count;

            // return
            return allItems[pos];
 
        }

    }
}

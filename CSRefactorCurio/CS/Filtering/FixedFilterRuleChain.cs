using System.Collections.Generic;

namespace DataTools.CSTools
{
    /// <summary>
    /// A simple and open chained filter rule for strongly typed filters.
    /// </summary>
    internal abstract class FixedFilterRuleChain<TMarker, TList> : MarkerFilterRule<TMarker, TList>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
    {
        private MarkerFilterRuleChain<TMarker, TList> filterChain;

        /// <summary>
        /// Create a new fixed filter rule chain.
        /// </summary>
        public FixedFilterRuleChain()
        {
            filterChain = new MarkerFilterRuleChain<TMarker, TList>();
            filterChain.FilterChainKind = FilterChainKind;

            var newRules = ProvideFilterChain();

            foreach (var rule in newRules)
            {
                filterChain.RuleChain.Add(rule);
            }
        }

        /// <summary>
        /// Gets the kind of filter chain (validate any or validate all).
        /// </summary>
        public abstract FilterChainKind FilterChainKind { get; }

        /// <summary>
        /// Runs each filter in succession, using the results of the previous filter in the chain to run the next filter in the chain.
        /// </summary>
        /// <param name="items">The final list of items.</param>
        /// <returns></returns>
        public override TList ApplyFilter(TList items)
        {
            return filterChain.ApplyFilter(items);
        }

        /// <summary>
        /// Runs each element in the rule chain, in enumeration order.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>True if the item passes, otherwise false.</returns>
        public override bool IsValid(IMarker item)
        {
            return filterChain.IsValid(item);
        }

        /// <summary>
        /// Provide the filter chain.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<MarkerFilterRule<TMarker, TList>> ProvideFilterChain();
    }
}
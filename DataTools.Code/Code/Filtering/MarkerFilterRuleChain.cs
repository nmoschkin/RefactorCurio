using DataTools.Code.Filtering.Base;
using DataTools.Code.Markers;

using System.Collections.Generic;

namespace DataTools.Code.Filtering
{
    /// <summary>
    /// A simple and open chained filter rule.
    /// </summary>
    internal class MarkerFilterRuleChain : MarkerFilterRule
    {
        private List<MarkerFilterRule> rules;

        /// <summary>
        /// Create a new marker rule chain.
        /// </summary>
        public MarkerFilterRuleChain()
        {
            rules = new List<MarkerFilterRule>();
        }

        /// <summary>
        /// Create a new marker rule chain from the specified initial starting values.
        /// </summary>
        /// <param name="rules">The initial starting values.</param>
        public MarkerFilterRuleChain(IEnumerable<MarkerFilterRule> rules)
        {
            this.rules = new List<MarkerFilterRule>(rules);
        }

        /// <summary>
        /// Gets or sets the kind of chain (pass all or pass any).
        /// </summary>
        public virtual FilterPassMode FilterChainKind { get; set; } = FilterPassMode.PassAll;

        /// <summary>
        /// Gets or sets the rule chain that will be used to validate items.
        /// </summary>
        public virtual List<MarkerFilterRule> RuleChain
        {
            get => rules;
            set => rules = value;
        }

        /// <summary>
        /// Runs each element in the rule chain, in enumeration order.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>True if the item passes, otherwise false.</returns>
        public override bool IsValid(IMarker item)
        {
            if (FilterChainKind == FilterPassMode.PassAll)
            {
                foreach (var rule in rules)
                {
                    if (!rule.IsValid(item)) return false;
                }

                return true;
            }
            else if (FilterChainKind == FilterPassMode.PassAny)
            {
                foreach (var rule in rules)
                {
                    if (rule.IsValid(item)) return true;
                }

                return false;
            }

            return false;
        }
    }

    /// <summary>
    /// A simple and open chained filter rule for strongly typed filters.
    /// </summary>
    /// <typeparam name="TMarker">The type that implements <see cref="IMarker{TMarker, TList}"/>.</typeparam>
    /// <typeparam name="TList">The type that implements a <see cref="IMarkerList{TMarker}"/> for <see cref="IMarker{TMarker, TList}.Children"/>.</typeparam>
    internal class MarkerFilterRuleChain<TMarker, TList> : MarkerFilterRule<TMarker, TList>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
    {
        private List<MarkerFilterRule<TMarker, TList>> rules;

        /// <summary>
        /// Create a new marker rule chain.
        /// </summary>
        public MarkerFilterRuleChain()
        {
            rules = new List<MarkerFilterRule<TMarker, TList>>();
        }

        /// <summary>
        /// Create a new marker rule chain from the specified initial starting values.
        /// </summary>
        /// <param name="rules">The initial starting values.</param>
        public MarkerFilterRuleChain(IEnumerable<MarkerFilterRule<TMarker, TList>> rules)
        {
            this.rules = new List<MarkerFilterRule<TMarker, TList>>(rules);
        }

        /// <summary>
        /// Gets or sets the kind of chain (pass all or pass any).
        /// </summary>
        public virtual FilterPassMode FilterChainKind { get; set; } = FilterPassMode.PassAll;

        /// <summary>
        /// Gets or sets the rule chain that will be used to validate items.
        /// </summary>
        public virtual List<MarkerFilterRule<TMarker, TList>> RuleChain
        {
            get => rules;
            set => rules = value;
        }

        /// <summary>
        /// Runs each filter in succession, using the results of the previous filter in the chain to run the next filter in the chain.
        /// </summary>
        /// <param name="items">The final list of items.</param>
        /// <returns></returns>
        public override TList ApplyFilter(TList items)
        {
            TList result = items;

            foreach (var rule in rules)
            {
                result = rule.ApplyFilter(result);
            }

            return result;
        }

        /// <summary>
        /// Runs each element in the rule chain, in enumeration order.
        /// </summary>
        /// <param name="items">The items to test.</param>
        /// <returns>True if all items pass, otherwise false.</returns>
        public virtual bool IsValid(IEnumerable<TMarker> items)
        {
            foreach (var item in items)
            {
                if (!IsValid(item)) return false;
            }

            return true;
        }

        /// <summary>
        /// Runs each element in the rule chain, in enumeration order.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>True if the item passes, otherwise false.</returns>
        public override bool IsValid(IMarker item)
        {
            if (FilterChainKind == FilterPassMode.PassAll)
            {
                foreach (var rule in rules)
                {
                    if (!rule.IsValid(item)) return false;
                }

                return true;
            }
            else if (FilterChainKind == FilterPassMode.PassAny)
            {
                foreach (var rule in rules)
                {
                    if (rule.IsValid(item)) return true;
                }

                return false;
            }

            return false;
        }
    }
}
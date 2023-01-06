using DataTools.Code.Filtering;
using DataTools.Code.Filtering.Base;
using DataTools.Code.Markers;

using System.Collections.Generic;

namespace DataTools.Code.CS.Filtering
{
    /// <summary>
    /// The default display filter chain for consisting of the XML Eliminator and the File Sorter/Filter.
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    /// <typeparam name="TList"></typeparam>
    internal class CSProjectDisplayChain<TMarker, TList> : FixedFilterRuleChain<TMarker, TList>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
    {
        private IEnumerable<MarkerFilterRule<TMarker, TList>> extraFilters;

        public override FilterChainKind FilterChainKind => FilterChainKind.PassAll;

        /// <summary>
        /// Create a new <see cref="CSProjectDisplayChain{TMarker, TList}"/> filter chain with optional additional filters.
        /// </summary>
        /// <param name="extraFilters">Additional filters.</param>
        public CSProjectDisplayChain(IEnumerable<MarkerFilterRule<TMarker, TList>> extraFilters = null) : base()
        {
            this.extraFilters = extraFilters;
        }

        protected override sealed IEnumerable<MarkerFilterRule<TMarker, TList>> ProvideFilterChain()
        {
            var l = new List<MarkerFilterRule<TMarker, TList>>
            {
                new CSXMLEliminatorFilter<TMarker, TList>(),
                new CSFileSortFilter<TMarker, TList>()
            };

            if (extraFilters != null)
            {
                l.AddRange(extraFilters);
            }

            return l.ToArray();
        }
    }
}
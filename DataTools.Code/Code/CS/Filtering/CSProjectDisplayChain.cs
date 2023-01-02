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
        public override FilterChainKind FilterChainKind => FilterChainKind.PassAll;

        protected override IEnumerable<MarkerFilterRule<TMarker, TList>> ProvideFilterChain()
        {
            return new MarkerFilterRule<TMarker, TList>[]
            {
                new CSXMLEliminatorFilter<TMarker, TList>(),
                new CSFileSortFilter<TMarker, TList>()
            };
        }
    }
}
using DataTools.Code.Filtering.Base;
using DataTools.Code.Markers;

using System.Linq;

namespace DataTools.Code.CS.Filtering
{
    /// <summary>
    /// Eliminates XML Document tags and comments and disintegrates merged compartments.
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    /// <typeparam name="TList"></typeparam>
    internal class CSXMLEliminatorFilter<TMarker, TList> : DeepFilterRule<TMarker, TList>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
    {
        public override bool IsValid(IMarker item)
        {
            return item.Kind != MarkerKind.XMLDoc && item.Kind != MarkerKind.BlockComment && item.Kind != MarkerKind.LineComment;
        }
    }
}
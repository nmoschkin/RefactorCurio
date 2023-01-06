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
    internal class CSXMLEliminatorFilter<TMarker, TList> : MarkerFilterRule<TMarker, TList>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
    {
        public override TList ApplyFilter(TList items)
        {
            var l = new TList();

            foreach (var p1item in items)
            {
                if (IsValid(p1item))
                {
                    var cItem = p1item.Clone<TMarker>(false);

                    if ((cItem.Kind & CodeElementType.IsBlockLevel) == CodeElementType.IsBlockLevel)
                    {
                        cItem.Children = new TList();
                    }

                    l.Add(cItem);
                }
            }

            int i, c = l.Count;

            for (i = 0; i < c; i++)
            {
                var p2item = l[i];

                if ((p2item.Kind & CodeElementType.IsBlockLevel) == CodeElementType.IsBlockLevel)
                {
                    p2item.Children = new TList();
                }

                var l2 = new TList();

                foreach (var child in p2item.Children)
                {
                    if (IsValid(child))
                    {
                        var cItem = child.Clone<TMarker>(false);

                        if ((cItem.Kind & CodeElementType.IsBlockLevel) == CodeElementType.IsBlockLevel)
                        {
                            cItem.Children = new TList();
                        }

                        l2.Add(cItem);
                    }
                }

                if (p2item.Kind == CodeElementType.Consolidation)
                {
                    l[i] = l2.Last();
                }

                l[i].Children = ApplyFilter(l[i].Children);
            }

            return l;
        }

        public override bool IsValid(IMarker item)
        {
            return item.Kind != CodeElementType.XMLDoc && item.Kind != CodeElementType.BlockComment && item.Kind != CodeElementType.LineComment;
        }
    }
}
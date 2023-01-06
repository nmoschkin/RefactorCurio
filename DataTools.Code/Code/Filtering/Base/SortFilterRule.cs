using DataTools.Code.Markers;
using DataTools.Essentials.SortedLists;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTools.Code.Filtering.Base
{
    /// <summary>
    /// Base class for a strongly-typed, self-applying sort filter.
    /// </summary>
    /// <typeparam name="TMarker">The <see cref="IMarker"/> element type.</typeparam>
    /// <typeparam name="TList">The <see cref="IMarkerList{TMarker}"/> type.</typeparam>
    internal abstract class SortFilterRule<TMarker, TList> : MarkerFilterRule<TMarker, TList>, IComparer<TMarker>
        where TMarker : IMarker, new()
        where TList : IMarkerList<TMarker>, new()
    {
        public override TList ApplyFilter(TList items)
        {
            var newItems = new TList();
            foreach (var item in items)
            {
                if (IsValid(item))
                {
                    newItems.Add(item);
                }
            }

            QuickSort.Sort(newItems, Compare);
            return newItems;
        }

        public abstract int Compare(TMarker x, TMarker y);
    }
}
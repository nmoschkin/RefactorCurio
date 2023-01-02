using DataTools.Code.Markers;
using DataTools.Essentials.SortedLists;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTools.Code.Filtering.Base
{
    internal static class DefaultOrders
    {
        /// <summary>
        /// Gets the default sort order for filters, lists, and rules.
        /// </summary>
        public static readonly MarkerKind[] DefaultSortOrder = new MarkerKind[] {
            MarkerKind.Interface,
            MarkerKind.Class,
            MarkerKind.Record,
            MarkerKind.Struct,
            MarkerKind.Enum,
            MarkerKind.Const,
            MarkerKind.Delegate,
            MarkerKind.Constructor,
            MarkerKind.Destructor,
            MarkerKind.Method,
            MarkerKind.Property,
            MarkerKind.Field,
            MarkerKind.Operator,
            MarkerKind.EnumValue,
            MarkerKind.FieldValue,
            MarkerKind.Event,
            MarkerKind.Get,
            MarkerKind.Set,
            MarkerKind.Add,
            MarkerKind.Remove
        };

        public static readonly MarkerKind[] DefaultFQNFilter = new[]
        {
            MarkerKind.Namespace,
            MarkerKind.Class,
            MarkerKind.Interface,
            MarkerKind.Struct,
            MarkerKind.Record,
            MarkerKind.Enum,
            MarkerKind.Method,
            MarkerKind.Property,
            MarkerKind.Delegate,
            MarkerKind.Const,
            MarkerKind.Get,
            MarkerKind.Set,
            MarkerKind.Add,
            MarkerKind.Remove,
        };
    }

    /// <summary>
    /// Base class for a strongly-typed, self-applying sort filter.
    /// </summary>
    /// <typeparam name="TMarker">The <see cref="IMarker"/> element type.</typeparam>
    /// <typeparam name="TList">The <see cref="IMarkerList{TMarker}"/> type.</typeparam>
    internal abstract class SortFilterRule<TMarker, TList> : MarkerFilterRule<TMarker, TList>, IComparer<TMarker> where TMarker : IMarker, new() where TList : IMarkerList<TMarker>, new()
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
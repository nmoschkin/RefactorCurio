using DataTools.Code.Filtering.Base;
using DataTools.Code.Markers;

using System;
using System.Collections.Generic;

namespace DataTools.Code.CS.Filtering
{
    /// <summary>
    /// File marker sort and filter.
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    /// <typeparam name="TList"></typeparam>
    internal class CSFileSortFilter<TMarker, TList> : SortFilterRule<TMarker, TList>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
    {
        private int m = 1;
        private bool descending = false;

        public override TList ApplyFilter(TList items)
        {
            var rl = new TList();

            foreach (var item in items)
            {
                if (item.Kind == CodeElementType.Namespace)
                {
                    foreach (var subitem in item.Children)
                    {
                        if (((IList<CodeElementType>)SortKindOrder).Contains(subitem.Kind))
                        {
                            rl.Add(subitem.Clone<TMarker>(false));
                        }
                    }
                }
                else if (((IList<CodeElementType>)SortKindOrder).Contains(item.Kind))
                {
                    rl.Add(item.Clone<TMarker>(false));
                }
            }

            foreach (var item in rl)
            {
                item.Children = ApplyFilter(item.Children);
            }

            return base.ApplyFilter(rl);
        }

        /// <summary>
        /// Gets or sets the sort kind order for this filter.
        /// Items will be sorted according to kind, and being present in the list conveys an item kind's validity.
        /// </summary>
        public CodeElementType[] SortKindOrder { get; set; } = DefaultOrders.DefaultSortOrder;

        public override bool IsValid(IMarker item)
        {
            return ((IList<CodeElementType>)SortKindOrder).Contains(item.Kind);
        }

        public bool Descending
        {
            get => descending;
            set
            {
                if (descending != value)
                {
                    descending = value;
                    m = Descending ? -1 : 1;
                }
            }
        }

        public override int Compare(TMarker x, TMarker y)
        {
            if (x.Kind == y.Kind)
            {
                return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) * m;
            }
            else
            {
                var sk = SortKindOrder as IList<CodeElementType>;

                var a = sk.IndexOf(x.Kind);
                var b = sk.IndexOf(y.Kind);

                if (a < b) return -1 * m;
                if (a > b) return 1 * m;

                return 0;
            }
        }

        public CSFileSortFilter()
        {
            Descending = false;
        }

        public CSFileSortFilter(bool descending)
        {
            Descending = descending;
        }
    }
}
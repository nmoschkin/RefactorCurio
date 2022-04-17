using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTools.CSTools
{
    /// <summary>
    /// File sort filter.
    /// </summary>
    /// <typeparam name="TElem"></typeparam>
    /// <typeparam name="TList"></typeparam>
    public class CSFileSortFilter<TElem, TList> : SortFilterRule<TElem, TList>
        where TList : IMarkerList<TElem>, new()
        where TElem : IMarker<TElem, TList>, new()
    {
        int m = 1;
        bool descending = false;

        public override TList ApplyFilter(TList items)
        {
            var rl = new TList();
            
            foreach (var item in items)
            {
                if (item.Kind == MarkerKind.Namespace)
                {
                    foreach (var subitem in item.Children)
                    {
                        if (((IList<MarkerKind>)SortKindOrder).Contains(subitem.Kind))
                            rl.Add(subitem);
                    }
                }
                else if (((IList<MarkerKind>)SortKindOrder).Contains(item.Kind))
                {
                    rl.Add(item);
                }
            }

            return base.ApplyFilter(rl);
        }

        /// <summary>
        /// Gets or sets the sort kind order for this filter.  
        /// Items will be sorted according to kind, and being present in the list conveys an item kind's validity.
        /// </summary>
        public MarkerKind[] SortKindOrder { get; set; } = new MarkerKind[] {                 
            MarkerKind.Interface,
            MarkerKind.Class,
            MarkerKind.Record,
            MarkerKind.Struct,
            MarkerKind.Enum,
            MarkerKind.Delegate,
        };

        public override bool IsValid(IMarker item)
        {
            switch (item.Kind)
            {
                case MarkerKind.Interface:
                case MarkerKind.Class:
                case MarkerKind.Record:
                case MarkerKind.Struct:
                case MarkerKind.Enum:
                case MarkerKind.Delegate:
                    return true;

                default:
                    return false;
            }
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
        public override int Compare(TElem x, TElem y)
        {
            if (x.Kind == y.Kind)
            {
                return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) * m;
            }
            else
            {
                var sk = SortKindOrder as IList<MarkerKind>;

                return (sk.IndexOf(x.Kind) - sk.IndexOf(y.Kind)) * m;
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

    public class CSXMLEliminatorFilter<TElem, TList> : MarkerFilterRule<TElem, TList>
        where TList : IMarkerList<TElem>, new()
        where TElem : IMarker<TElem, TList>, new()
    {
        public override TList ApplyFilter(TList items)
        {
            var l = new TList();

            foreach (var p1item in items)
            {
                if (IsValid(p1item))
                {
                    l.Add(p1item.Clone<TElem>(false));
                }
            }

            int i, c = l.Count;

            for (i = 0; i < c; i++) 
            {
                var p2item = l[i];
                var l2 = new TList();

                foreach (var child in p2item.Children)
                {
                    if (IsValid(child))
                    {
                        l2.Add(child.Clone<TElem>(false));
                    }
                }

                if (l2.Count == 1)
                {
                    l[i] = l2[0];
                }

                l[i].Children = ApplyFilter(l[i].Children);
            }

            return l;
        }

        public override bool IsValid(IMarker item)
        {
            return item.Kind != MarkerKind.XMLDoc && item.Kind != MarkerKind.BlockComment && item.Kind != MarkerKind.LineComment;
        }
    }

    public class CSFileChain<TElem, TList> : FixedFilterRuleChain<TElem, TList>
        where TList : IMarkerList<TElem>, new()
        where TElem : IMarker<TElem, TList>, new()
    {
        public override FilterChainKind FilterChainKind => FilterChainKind.PassAll;

        protected override IEnumerable<MarkerFilterRule<TElem, TList>> ProvideFilterChain()
        {
            return new MarkerFilterRule<TElem, TList>[]
            {
                new CSXMLEliminatorFilter<TElem, TList>(),
                new CSFileSortFilter<TElem, TList>()
            };
        }
    }

}

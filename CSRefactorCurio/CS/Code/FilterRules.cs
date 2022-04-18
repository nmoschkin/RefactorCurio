using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTools.CSTools
{
    /// <summary>
    /// File marker sort and filter.
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
                        {
                            rl.Add(subitem.Clone<TElem>(false));
                        }
                    }
                }
                else if (((IList<MarkerKind>)SortKindOrder).Contains(item.Kind))
                {
                    rl.Add(item.Clone<TElem>(false));
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
        public MarkerKind[] SortKindOrder { get; set; } = new MarkerKind[] {                 
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
            MarkerKind.EnumValue,
            MarkerKind.FieldValue,
            MarkerKind.Event,
        };

        public override bool IsValid(IMarker item)
        {
            return ((IList<MarkerKind>)SortKindOrder).Contains(item.Kind);
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

    /// <summary>
    /// Eliminates XML Document tags and comments and disintegrates merged compartments.
    /// </summary>
    /// <typeparam name="TElem"></typeparam>
    /// <typeparam name="TList"></typeparam>
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
                    var cItem = p1item.Clone<TElem>(false);

                    if ((cItem.Kind & MarkerKind.IsBlockLevel) == MarkerKind.IsBlockLevel)
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

                if ((p2item.Kind & MarkerKind.IsBlockLevel) == MarkerKind.IsBlockLevel)
                {
                    p2item.Children = new TList();
                }

                var l2 = new TList();
                
                foreach (var child in p2item.Children)
                {
                    if (IsValid(child))
                    {
                        var cItem = child.Clone<TElem>(false);
                        
                        if ((cItem.Kind & MarkerKind.IsBlockLevel) == MarkerKind.IsBlockLevel)
                        {
                            cItem.Children = new TList();
                        }

                        l2.Add(cItem);
                    }
                }

                if (p2item.Kind == MarkerKind.Consolidation)
                {
                    l[i] = l2.Last();
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

    /// <summary>
    /// The default filter chain for files consisting of the XML Eliminator and the File Sorter/Filter.
    /// </summary>
    /// <typeparam name="TElem"></typeparam>
    /// <typeparam name="TList"></typeparam>
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

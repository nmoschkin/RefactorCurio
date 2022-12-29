using CSRefactorCurio.Reporting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataTools.CSTools
{
    /// <summary>
    /// File marker sort and filter.
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    /// <typeparam name="TList"></typeparam>
    public class CSFileSortFilter<TMarker, TList> : SortFilterRule<TMarker, TList>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
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
                            rl.Add(subitem.Clone<TMarker>(false));
                        }
                    }
                }
                else if (((IList<MarkerKind>)SortKindOrder).Contains(item.Kind))
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
        public MarkerKind[] SortKindOrder { get; set; } = ReportHelper.DefaultSortOrder;

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
        public override int Compare(TMarker x, TMarker y)
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
    /// <typeparam name="TMarker"></typeparam>
    /// <typeparam name="TList"></typeparam>
    public class CSXMLEliminatorFilter<TMarker, TList> : MarkerFilterRule<TMarker, TList>
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
                        var cItem = child.Clone<TMarker>(false);
                        
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
    /// The default display filter chain for consisting of the XML Eliminator and the File Sorter/Filter.
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    /// <typeparam name="TList"></typeparam>
    public class CSProjectDisplayChain<TMarker, TList> : FixedFilterRuleChain<TMarker, TList>
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


    /// <summary>
    /// Eliminates XML Document tags and comments and disintegrates merged compartments.
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    /// <typeparam name="TList"></typeparam>
    public class CSXMLIntegratorFilter<TMarker, TList> : MarkerFilterRule<TMarker, TList>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
    {

        private TMarker FindEndIf(TList markers, int index, int cdepth, out int? relativeIndex)
        {
            int c = markers.Count;

            for (int i = index; i < c; i++)
            {
                if (markers[i].Kind == MarkerKind.Directive && markers[i].Name.StartsWith("#if"))
                {
                    cdepth++;
                }
                else if (markers[i].Kind == MarkerKind.Directive && markers[i].Name == "#endif")
                {
                    cdepth--;
                    if (cdepth == 0)
                    {
                        relativeIndex = i;
                        return markers[i];
                    }
                }

                if (markers[i].Children != null && markers[i].Children.Count > 0)
                {
                    int? re;
                    var result = FindEndIf(markers[i].Children, 0, cdepth, out re);
                    if (result != null)
                    {
                        relativeIndex = i;
                        return result;
                    }
                }

            }

            relativeIndex = null;
            return default;
        }


        public override TList ApplyFilter(TList markers)
        {
            int i, c = markers.Count;
            var lnew = new TList();

            for (i = 0; i < c; i++)
            {
                if (markers[i].Kind == MarkerKind.Directive && markers[i].Name.StartsWith("#if "))
                {
                    var xmarker = FindEndIf(markers, i, 0, out int? ri);
                    
                    if (xmarker != null && ri is int rix)
                    {
                        int j;

                        for (j = i; j <= rix; j++)
                        {
                            if (ReportHelper.DefaultSortOrder.Contains(markers[j].Kind)) break;
                        }

                        if (j <= rix)
                        {
                            var mknew = markers[j].Clone<TMarker>(false);

                            mknew.StartPos = markers[i].StartPos;
                            mknew.StartLine = markers[i].StartLine;
                            mknew.StartColumn = markers[i].StartColumn;

                            mknew.Children = new TList();

                            for (int z = i; z <= rix; z++)
                            {
                                if (markers[z].Children != null && markers[z].Children.Count > 0)
                                {
                                    markers[z].Children = ApplyFilter(markers[z].Children);
                                }

                                mknew.Children.Add(markers[z]);
                            }

                            mknew.EndPos = markers[rix].EndPos;
                            mknew.EndLine = markers[rix].EndLine;
                            mknew.EndColumn = markers[rix].EndColumn;

                            //mknew.Kind = MarkerKind.Consolidation;
                            lnew.Add(mknew);
                            i = rix;

                            continue;
                        }
                    }
                }

                if (markers[i].Kind == MarkerKind.XMLDoc || markers[i].Kind == MarkerKind.LineComment || markers[i].Kind == MarkerKind.BlockComment)
                {
                    int oi = i;
                    int x = i;

                    while (i < c && (markers[i].Kind == MarkerKind.XMLDoc || markers[i].Kind == MarkerKind.LineComment || markers[i].Kind == MarkerKind.BlockComment))
                    {
                        i++;
                    }

                    if (i < c)
                    {
                        var mknew = markers[i].Clone<TMarker>(false);

                        mknew.StartPos = markers[x].StartPos;
                        mknew.StartLine = markers[x].StartLine;
                        mknew.StartColumn = markers[x].StartColumn;

                        mknew.Children = new TList();

                        for (int z = x; z <= i; z++)
                        {
                            if (markers[z].Children != null && markers[z].Children.Count > 0)
                            {
                                markers[z].Children = ApplyFilter(markers[z].Children);
                            }

                            mknew.Children.Add(markers[z]);
                        }

                        mknew.EndPos = markers[i].EndPos;
                        mknew.EndLine = markers[i].EndLine;
                        mknew.EndColumn = markers[i].EndColumn;

                        //mknew.Kind = MarkerKind.Consolidation;
                        lnew.Add(mknew);
                    }
                    else
                    {
                        i = oi;
                        continue;
                    }
                }
                else
                {
                    switch(markers[i].Kind)
                    {
                        case MarkerKind.Namespace:
                            foreach (var newItem in ApplyFilter(markers[i].Children)) lnew.Add(newItem);
                            break;

                        case MarkerKind.Using:
                            break;

                        default:
                            var cmarker = markers[i].Clone<TMarker>(false);
                            if (cmarker.Children != null && cmarker.Children.Count > 0)
                            {
                                cmarker.Children = ApplyFilter(cmarker.Children);
                            }

                            lnew.Add(cmarker);
                            break;

                    }
                }
            }

            return lnew;

        }

        public override bool IsValid(IMarker item)
        {
            return true;
        }
    }

    /// <summary>
    /// The default file filter chain for consisting of the XML combiner and the File Sorter/Filter.
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    /// <typeparam name="TList"></typeparam>
    public class CSFileChain<TMarker, TList> : FixedFilterRuleChain<TMarker, TList>
        where TList : IMarkerList<TMarker>, new()
        where TMarker : IMarker<TMarker, TList>, new()
    {
        public override FilterChainKind FilterChainKind => FilterChainKind.PassAll;

        protected override IEnumerable<MarkerFilterRule<TMarker, TList>> ProvideFilterChain()
        {
            return new MarkerFilterRule<TMarker, TList>[]
            {
                new CSXMLIntegratorFilter<TMarker, TList>(),
                new CSFileSortFilter<TMarker, TList>()
            };
        }
    }



}

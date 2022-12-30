using CSRefactorCurio.Reporting;

using System.Linq;

namespace DataTools.CSTools
{
    /// <summary>
    /// Eliminates XML Document tags and comments and disintegrates merged compartments.
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    /// <typeparam name="TList"></typeparam>
    internal class CSXMLIntegratorFilter<TMarker, TList> : MarkerFilterRule<TMarker, TList>
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
                    switch (markers[i].Kind)
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
}
using CSRefactorCurio.Globalization.Resources;

using DataTools.Code.Filtering.Base;
using DataTools.Code.Markers;
using DataTools.Code.Project;
using DataTools.Code.Reporting;
using DataTools.CSTools;
using DataTools.Essentials.SortedLists;

using System.Collections.Generic;
using System.ComponentModel;

namespace CSRefactorCurio.Reporting
{
    internal class NamespaceReportNode : ReportNode<INamespace>
    {
    }

    internal class CountReferencesReport : ReportBase<INamespace, NamespaceReportNode>
    {
        [Browsable(true)]
        public override string ReportName { get; } = AppResources.REPORT_COUNT_REFERENCES;

        [Browsable(true)]
        public override string AssociatedReason { get; }

        [Browsable(true)]
        public override int ReportId { get; } = 1;

        public CountReferencesReport(ISolution solution, string associated) : base(solution)
        {
            AssociatedReason = associated;
        }

        public CountReferencesReport(ISolution solution) : this(solution, "DEFAULT")
        {
        }

        public override void CompileReport(IEnumerable<INamespace> context)
        {            
            var allFQN = ReportHelper.AllFullyQualifiedNames(context);

            var allref = ReportHelper.GetReferences(Solution.Projects, allFQN);
            var so = (IList<MarkerKind>)DefaultOrders.DefaultSortOrder;

            allref.Sort((a, b) =>
            {
                if (a.Equals(b)) return 0;

                var x = so.IndexOf(a.ReferencedObject.Kind);
                var y = so.IndexOf(b.ReferencedObject.Kind);

                if (x != -1 && y != -1)
                {
                    if (x < y) return -1;
                    if (y < x) return 1;
                }

                x = so.IndexOf(a.CallingObject.Kind);
                y = so.IndexOf(b.CallingObject.Kind);

                if (x != -1 && y != -1)
                {
                    if (x < y) return -1;
                    if (y < x) return 1;
                }

                var c = string.Compare(a.ReferencedObject.FullyQualifiedName, b.ReferencedObject.FullyQualifiedName);

                if (c == 0)
                {
                    c = string.Compare(a.ReferencedObject.Generics, b.ReferencedObject.Generics);

                    if (c == 0)
                    {
                        c = string.Compare(a.CallingObject.FullyQualifiedName, b.CallingObject.FullyQualifiedName);

                        if (c == 0)
                        {
                            c = string.Compare(a.ReferencedObject.Generics, b.ReferencedObject.Generics);
                        }
                    }
                }

                return c;
            });

            var rpts = new List<NamespaceReportNode>();

            CSMarker curr = null;

            List<CSMarker> markers = new List<CSMarker>();

            foreach (var item in allref)
            {
                if (curr != item.ReferencedObject)
                {
                    if (markers.Count > 0)
                    {
                        var rpt = new NamespaceReportNode()
                        {
                            AssociatedList = markers.ToArray(),
                            Element = curr
                        };

                        rpts.Add(rpt);
                        markers = new List<CSMarker>();
                    }

                    curr = item.ReferencedObject;
                }

                if (!markers.Contains(item.CallingObject)) markers.Add(item.CallingObject);
            }

            if (curr != null)
            {
                if (markers.Count > 0)
                {
                    var rpt = new NamespaceReportNode()
                    {
                        AssociatedList = markers.ToArray(),
                        Element = curr
                    };

                    rpts.Add(rpt);
                    markers = new List<CSMarker>();
                }
            }

            Reports = rpts;
            Sort();
        }

        public override void Sort()
        {
            QuickSort.Sort(Reports, (a, b) =>
            {
                if (a.AssociatedList.Count > b.AssociatedList.Count) return -1;
                if (a.AssociatedList.Count < b.AssociatedList.Count) return 1;
                return 0;
            });
        }
    }
}
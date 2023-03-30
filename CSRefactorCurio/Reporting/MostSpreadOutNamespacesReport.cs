using CSRefactorCurio.Globalization.Resources;

using DataTools.Code.Project;
using DataTools.Code.Reporting;
using DataTools.CSTools;
using DataTools.Essentials.SortedLists;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CSRefactorCurio.Reporting
{
    internal delegate bool ItemFilterFunc(INamespace item);

    internal class ProjectReportNode : ReportNode<IProjectNode>
    {
        public int TypeCount { get; protected internal set; }

        public override string ToString()
        {            
            return $"{base.ToString()} ({AssociatedList.Count} {AppResources.FILES}) ({TypeCount} {AppResources.TYPES})";
        }
    }

    internal class MostSpreadOutNamespacesReport : ReportBase<ProjectReportNode>
    {
        [Browsable(true)]
        public override string ReportName { get; } = AppResources.REPORT_MOST_SPREAD_OUT;

        [Browsable(true)]
        public override string AssociatedReason { get; }

        [Browsable(true)]
        public override int ReportId { get; } = 1;

        public MostSpreadOutNamespacesReport(ISolution solution, string associated) : base(solution)
        {
            AssociatedReason = associated;
        }

        public MostSpreadOutNamespacesReport(ISolution solution) : this(solution, "DEFAULT")
        {
        }

        public override void CompileReport<T>(IList<T> context)
        {
            var allFQN = ReportHelper.AllFullyQualifiedNames(context);

            var allref = ReportHelper.CountFilesForNamespaces(allFQN);

            var rpts = new List<ProjectReportNode>();

            List<CSMarker> markers = new List<CSMarker>();

            foreach (var item in allref)
            {
                var rpt = new ProjectReportNode()
                {
                    AssociatedList = new List<IProjectNode>(item.Value.Select((x) => (IProjectNode)x)),
                    Element = allFQN.Where((x) => x.Key == item.Key).First().Value.First()                    
                };

                rpt.TypeCount = rpt.AssociatedList.Sum((x) => ((CSCodeFile)x).GetAllTypes<List<CSMarker>>()?.Count ?? 0);
                rpts.Add(rpt);
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
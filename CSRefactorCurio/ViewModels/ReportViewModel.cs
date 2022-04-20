using CSRefactorCurio.Globalization.Resources;

using CSRefactorCurio.Reporting;

using DataTools.CSTools;
using DataTools.Observable;


using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CSRefactorCurio.ViewModels
{


    internal class ReportViewModel : ObservableBase, ICommandOwner
    {
        ISolution solution;

        public string[] ReportTypes { get; }

        public ISolution Solution => solution;

        private IOwnedCommand runReport;
        private CSReportBase selReport;
        private ObservableCollection<CSReportBase> reports = new ObservableCollection<CSReportBase>();

        public ObservableCollection<CSReportBase> Reports => reports;

        public IOwnedCommand RunReportCommand => runReport;

        public CSReportBase SelectedReport
        {
            get => selReport;
            set
            {
                SetProperty(ref selReport, value);
            }
        }

        public ReportViewModel(ISolution solution) 
        {
            ReportTypes = new string[] { AppResources.REPORT_MOST_INTERDEPENDENT, AppResources.REPORT_MOST_REFERENCED_OBJECTS, AppResources.REPORT_MOST_SPREAD_OUT };
            this.solution = solution;

            reports.Add(new HeaviestReferencesReport(this.Solution));
            runReport = new OwnedCommand(this, (o) =>
            {
                var b = solution.Namespaces.Where(o => o is INamespace).Select(o => o as INamespace).ToList();
                SelectedReport.CompileReport(b);
            }, nameof(RunReportCommand));
        }

        public ReportViewModel() : this(CSRefactorCurioPackage.Instance.CurioSolution)
        {
        }

        public bool RequestCanExecute(string commandId)
        {
            return true;
        }
    }
}

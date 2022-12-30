using CSRefactorCurio.Globalization.Resources;

using CSRefactorCurio.Reporting;

using DataTools.Code.Project;
using DataTools.Essentials.Observable;

using System.Collections.ObjectModel;
using System.Linq;

namespace CSRefactorCurio.ViewModels
{
    internal class ReportViewModel : ObservableBase, ICommandOwner
    {
        private ISolution solution;

        public string[] ReportTypes { get; }

        public ISolution Solution => solution;

        private IOwnedCommand runReport;
        private ReportBase selReport;
        private ObservableCollection<ReportBase> reports = new ObservableCollection<ReportBase>();

        public ObservableCollection<ReportBase> Reports => reports;

        public IOwnedCommand RunReportCommand => runReport;

        public ReportBase SelectedReport
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
            reports.Add(new MostSpreadOutNamespacesReport(this.Solution));
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
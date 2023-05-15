using CSRefactorCurio.Globalization.Resources;

using CSRefactorCurio.Reporting;

using DataTools.Code.Project;
using DataTools.Code.Reporting;

using System.Collections.ObjectModel;
using System.Linq;

namespace CSRefactorCurio.ViewModels
{
    internal class ReportViewModel : ViewModelBase
    {
        public virtual event EventHandler<RequestCloseEventArgs> RequestClose;

        private ISolution solution;

        //public string[] ReportTypes { get; }

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

        public ReportViewModel(ISolution solution) : base(false, false, false, false)
        {
            //ReportTypes = new string[] { AppResources.REPORT_MOST_INTERDEPENDENT, AppResources.REPORT_COUNT_REFERENCES, AppResources.REPORT_NAMESPACE_DISTRIBUTION };
            this.solution = solution;

            reports.Add(new CountReferencesReport(this.Solution));
            reports.Add(new NamespaceDistributionReport(this.Solution));

            runReport = new OwnedCommand(this, (o) =>
            {
                if (SelectedReport is ReportBase<INamespace> selrpt)
                {
                    var b = solution.Namespaces.Where(o => o is INamespace).Select(o => o as INamespace).ToList();
                    selrpt.CompileReport(b);
                }
            }, nameof(RunReportCommand));

            AutoRegisterCommands(this);
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
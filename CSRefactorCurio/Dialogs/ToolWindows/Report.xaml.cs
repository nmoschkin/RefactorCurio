using CSRefactorCurio.ViewModels;

using DataTools.CSTools;

using Microsoft.VisualStudio.PlatformUI;

namespace CSRefactorCurio.Dialogs
{
    public partial class Report : DialogWindow
    {
        ReportViewModel vm;
        public Report(ISolution solution)
        {
            InitializeComponent();
            DataContext = vm = new ReportViewModel(solution);
        }

    }

}

using CSRefactorCurio.Helpers;
using CSRefactorCurio.ViewModels;

using DataTools.CSTools;

using Microsoft.VisualStudio.PlatformUI;

namespace CSRefactorCurio.Dialogs
{
    public partial class Report : DialogWindow
    {
        private ReportViewModel vm;

        internal Report(ISolution solution)
        {
            InitializeComponent();
            DataContext = vm = new ReportViewModel(solution);

            Loaded += Report_Loaded;
        }

        private void Report_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ThemeHelper.ApplyVSTheme(this.Content);
        }
    }
}
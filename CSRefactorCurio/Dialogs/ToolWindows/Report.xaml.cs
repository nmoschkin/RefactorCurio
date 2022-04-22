using CSRefactorCurio.Helpers;
using CSRefactorCurio.ViewModels;

using DataTools.CSTools;

using Microsoft.VisualStudio.PlatformUI;

using System.Windows.Controls;
using System.Windows.Markup;

namespace CSRefactorCurio.Dialogs
{
    public partial class Report : DialogWindow
    {
        ReportViewModel vm;
        public Report(ISolution solution)
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

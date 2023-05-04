using CSRefactorCurio.Helpers;
using CSRefactorCurio.ViewModels;

using DataTools.Code.Project;
using DataTools.CSTools;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows;

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

        private void ProjTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //if (e.NewValue is CSMarker marker)
            //{
            //    selItem = vm.FindProjectItem(e.NewValue as IProjectNode);
            //    selMarker = marker;
            //}

            //vm.SelectedItem = e.NewValue;
        }

        private void ProjTree_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if (selItem != null && selMarker != null)
            //{
            //    var item = selItem;

            //    if (item == null) return;
            //    try
            //    {
            //        var window = item.Open();
            //        window.Activate();

            //        System.Threading.Thread.Sleep(50);

            //        EnvDTE.TextSelection ts = window.Selection as EnvDTE.TextSelection;
            //        ts.MoveToLineAndOffset(selMarker.StartLine, selMarker.StartColumn);
            //    }
            //    catch
            //    {
            //    }
            //}
        }
    }
}
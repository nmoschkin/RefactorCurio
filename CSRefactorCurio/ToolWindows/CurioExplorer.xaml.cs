using CSRefactorCurio.ViewModels;
using CSRefactorCurio.Helpers;

using DataTools.CSTools;

using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace CSRefactorCurio
{
    public partial class CurioExplorer : UserControl
    {
        CurioExplorerSolution vm;
        EnvDTE.ProjectItem selItem;
        CSMarker selMarker;

        public CurioExplorer()
        {
            InitializeComponent();
            Loaded += CurioExplorer_Loaded;
            lock (CSRefactorCurioPackage.SyncRoot)
            {
                DataContext = vm = CSRefactorCurioPackage.Instance.CurioSolution;
            }
        }

        private void CurioExplorer_Loaded(object sender, RoutedEventArgs e)
        {
        }
    

        private void ProjTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            
            if (e.NewValue is CSMarker marker)
            {
                selItem = vm.FindProjectItem(e.NewValue as IProjectNode);
                selMarker = marker; 

            }
        }

        private void ProjTree_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (selItem != null && selMarker != null)
            {
                var item = selItem;

                if (item == null) return;
                try
                {
                    var window = item.Open();
                    window.Activate();
                    Thread.Sleep(50);
                    window.DTE.ExecuteCommand("Edit.GoTo " + selMarker.StartLine.ToString());
                }
                catch
                {

                }
            }
        }
    }
}
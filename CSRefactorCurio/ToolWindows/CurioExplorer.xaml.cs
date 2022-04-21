using CSRefactorCurio.ViewModels;

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
            Button1.IsChecked = false;
            Button2.IsChecked = false;
            Button3.IsChecked = false;

            Button1.IsThreeState = false;
            Button2.IsThreeState = false;
            Button3.IsThreeState = false;

            lock (CSRefactorCurioPackage.SyncRoot)
            {
                DataContext = vm = CSRefactorCurioPackage.Instance.CurioSolution;
                vm.PropertyChanged += Vm_PropertyChanged;
            }
        }

        private void CurioExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            CheckClassMode(nameof(CurioExplorerSolution.ClassMode));
        }

        private void CheckClassMode(string propertyName)
        {
            Dispatcher.Invoke(() =>
            {
                if (propertyName == nameof(CurioExplorerSolution.ClassMode))
                {
                    if (vm.ClassMode == 0)
                    {
                        Button2.IsChecked = false;
                        Button3.IsChecked = false;
                    }
                    else if (vm.ClassMode == 1)
                    {
                        Button1.IsChecked = false;
                        Button3.IsChecked = false;
                    }
                    else if (vm.ClassMode == 2)
                    {
                        Button1.IsChecked = false;
                        Button2.IsChecked = false;
                    }
                }
            });

        }
        
        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CheckClassMode(nameof(CurioExplorerSolution.ClassMode));
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
                var window = item.Open();
                window.Activate();
                Thread.Sleep(50);
                window.DTE.ExecuteCommand("Edit.GoTo " + selMarker.StartLine.ToString());
            }
        }

        private void Button1_Checked(object sender, RoutedEventArgs e)
        {
            vm.ClassMode = 0;
        }

        private void Button2_Checked(object sender, RoutedEventArgs e)
        {
            vm.ClassMode = 1;
        }

        private void Button3_Checked(object sender, RoutedEventArgs e)
        {
            vm.ClassMode = 2;
        }
    }
}
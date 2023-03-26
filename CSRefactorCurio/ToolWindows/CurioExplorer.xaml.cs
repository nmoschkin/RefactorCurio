using CSRefactorCurio.Projects;

using DataTools.Code.Project;
using DataTools.CSTools;
using Microsoft.VisualStudio.Threading;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CSRefactorCurio
{
    public partial class CurioExplorer : UserControl
    {
        private CurioExplorerSolution vm;
        private EnvDTE.ProjectItem selItem;
        private CSMarker selMarker;

        public CurioExplorer()
        {
            InitializeComponent();
            Loaded += CurioExplorer_Loaded;
            TopBar.Loaded += ToolBar_Loaded;
            lock (CSRefactorCurioPackage.SyncRoot)
            {
                DataContext = vm = CSRefactorCurioPackage.Instance.CurioSolution;
                UpdateButtons(vm.ClassMode);
                vm.PropertyChanged += Vm_PropertyChanged;
            }
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurioExplorerSolution.ClassMode))
            {
                UpdateButtons(vm.ClassMode);
            }
        }

        private Dictionary<string, string> brushkeys = new Dictionary<string, string>();

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }

            //var props = typeof(VsBrushes).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            //var samctl = new Label();

            //foreach (var p in props)
            //{
            //    object obj;
            //    var s = (string)p.GetValue(null);

            //    samctl.SetResourceReference(Label.BackgroundProperty, s);

            //    if (samctl.Background is SolidColorBrush br)
            //    {
            //        brushkeys.Add(p.Name, br.Color.ToString());
            //    }
            //}

            //Clipboard.SetText(JsonConvert.SerializeObject(brushkeys));
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

            vm.SelectedItem = e.NewValue;
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

                    System.Threading.Thread.Sleep(50);

                    EnvDTE.TextSelection ts = window.Selection as EnvDTE.TextSelection;
                    ts.MoveToLineAndOffset(selMarker.StartLine, selMarker.StartColumn);
                }
                catch
                {
                }
            }
        }

        private async void UpdateButtons(int cm)
        {            
            Dispatcher.Invoke(() =>
            {
                BtnViewProject.IsChecked = cm == 0;
                BtnViewNamespace.IsChecked = cm == 1;
                BtnViewAuxTree.IsChecked = cm == 2;
            });
        }

        private void BtnViewProject_Click(object sender, RoutedEventArgs e)
        {
            vm.ClassMode = 0;
        }

        private void BtnViewNamespace_Click(object sender, RoutedEventArgs e)
        {
            vm.ClassMode = 1;
        }

        private void BtnViewAuxTree_Click(object sender, RoutedEventArgs e)
        {
            vm.ClassMode = 2;
        }

        private void BtnViewProject_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void BtnViewProject_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void BtnViewNamespace_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void BtnViewNamespace_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void BtnViewAuxTree_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void BtnViewAuxTree_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
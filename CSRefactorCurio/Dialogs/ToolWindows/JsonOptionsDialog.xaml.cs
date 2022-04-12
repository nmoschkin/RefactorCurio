using CSRefactorCurio.ViewModels;

using DataTools.CSTools;

using Microsoft.VisualStudio.PlatformUI;

namespace CSRefactorCurio.Dialogs
{
    public partial class JsonOptionsDialog : DialogWindow
    {

        JSConvertViewModel vm;

        public JsonOptionsDialog()
        {
            InitializeComponent();
            vm = new JSConvertViewModel();
            vm.RequestClose += Vm_RequestClose;
            DataContext = vm;
        }

        public JsonOptionsDialog(CurioProject project)
        {
            InitializeComponent();
            vm = new JSConvertViewModel(project);
            vm.RequestClose += Vm_RequestClose;
            DataContext = vm;
        }

        public JsonOptionsDialog(CurioProject project, string initPath)
        {
            InitializeComponent();
            vm = new JSConvertViewModel(project, initPath);
            vm.RequestClose += Vm_RequestClose;
            DataContext = vm;
        }

        private void Vm_RequestClose(object sender, RequestCloseEventArgs e)
        {
            DialogResult = e.IsSuccess;
            Close();
        }
    }
}

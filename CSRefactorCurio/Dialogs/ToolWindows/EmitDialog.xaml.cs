using CSRefactorCurio.ViewModels;

using DataTools.CSTools;

using Microsoft.VisualStudio.PlatformUI;

namespace CSRefactorCurio.Dialogs
{
    public partial class EmitDialog : DialogWindow
    {
        private EmitVMViewModel vm;

        internal EmitDialog(CSMarker source)
        {
            InitializeComponent();
            vm = new EmitVMViewModel(source);
            DataContext = vm;
            vm.RequestClose += Vm_RequestClose;
        }

        private void Vm_RequestClose(object sender, RequestCloseEventArgs e)
        {
            DialogResult = e.IsSuccess;
            Close();
        }
    }
}
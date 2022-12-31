using CSRefactorCurio.ViewModels;

using Microsoft.VisualStudio.PlatformUI;

using System;
using System.Linq;
using System.Text;

namespace CSRefactorCurio.Dialogs
{
    /// <summary>
    /// Interaction logic for TreeDataOptions.xaml
    /// </summary>
    public partial class TreeDataOptions : DialogWindow
    {
        private ExplorerFilterViewModel vm;

        internal TreeDataOptions(ExplorerFilterViewModel vm)
        {
            InitializeComponent();
            DataContext = this.vm = vm;
        }
    }
}
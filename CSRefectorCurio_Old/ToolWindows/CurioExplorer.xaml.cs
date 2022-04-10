
using CSRefectorCurio.ViewModels;

using DataTools.CSTools;

using EnvDTE80;
using EnvDTE;

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace CSRefectorCurio
{
    public partial class CurioExplorer : UserControl
    {
        [Import]
        internal static IServiceProvider ServiceProvider = null;
        
        public CurioExplorer()
        {
            InitializeComponent();
        }
    }
}
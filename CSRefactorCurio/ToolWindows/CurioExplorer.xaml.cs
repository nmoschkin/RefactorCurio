using System.Windows;
using System.Windows.Controls;

namespace CSRefactorCurio
{
    public partial class CurioExplorer : UserControl
    {
        public CurioExplorer()
        {
            InitializeComponent();
            DataContext = CSRefectorCurioPackage.Instance.CurioSolution;
        }

    }
}
using System.Windows;
using System.Windows.Controls;

namespace CSRefactorCurio
{
    public partial class CurioExplorer : UserControl
    {
        public CurioExplorer()
        {
            InitializeComponent();

            lock (CSRefactorCurioPackage.SyncRoot)
            {
                DataContext = CSRefactorCurioPackage.Instance.CurioSolution;
            }
        }

    }
}
using System.Windows;
using System.Windows.Controls;

namespace CSRefactorCurio
{
    public partial class CurioExplorer : UserControl
    {
        public CurioExplorer()
        {
            InitializeComponent();

            lock (CSRefectorCurioPackage.SyncRoot)
            {
                DataContext = CSRefectorCurioPackage.Instance.CurioSolution;
            }
        }

    }
}
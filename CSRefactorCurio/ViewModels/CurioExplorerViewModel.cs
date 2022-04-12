using DataTools.CSTools;
using DataTools.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRefactorCurio.ViewModels
{
    internal class CurioExplorerViewModel : ObservableBase
    {
        EnvDTE.Solution _sln;

        public CurioExplorerViewModel()
        {
        }

        public CurioExplorerViewModel(EnvDTE.Solution sln)
        {
            _sln = sln;
        }

        public EnvDTE.Solution Solution
        {
            get => _sln;
            internal set => _sln = value;
        }

        private ObservableCollection<CurioProject> projects = new ObservableCollection<CurioProject>();


        public ObservableCollection<CurioProject> Projects
        {
            get => projects;
        }

    }
}

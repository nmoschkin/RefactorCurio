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


        private ObservableCollection<CurioProject> projects = new ObservableCollection<CurioProject>();


        public ObservableCollection<CurioProject> Projects
        {
            get => projects;
        }






    }
}

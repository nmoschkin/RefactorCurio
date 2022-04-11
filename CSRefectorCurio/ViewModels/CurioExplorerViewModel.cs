using DataTools.CSTools;
using DataTools.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRefectorCurio.ViewModels
{
    internal class CurioExplorerViewModel : ObservableBase
    {


        private ObservableCollection<ProjectReader> projects = new ObservableCollection<ProjectReader>();


        public ObservableCollection<ProjectReader> Projects
        {
            get => projects;
        }






    }
}

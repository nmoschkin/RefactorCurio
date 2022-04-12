using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRefactorCurio
{
    public interface ICommandOwner : INotifyPropertyChanged
    {
        bool RequestCanExecute(string commandId);
    }
}

using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CSRefactorCurio
{
    internal interface ICommandOwner : INotifyPropertyChanged
    {
        bool RequestCanExecute(string commandId);
    }
}
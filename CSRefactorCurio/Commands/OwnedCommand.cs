using DataTools.Observable;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace CSRefactorCurio
{
    public delegate void ExecOwnedCommandHandler(object parameter);


    public class OwnedCommand : IOwnedCommand
    {
        public event EventHandler CanExecuteChanged;

        private string cmdId = Guid.NewGuid().ToString("d");

        private bool canExecute = true;

        private ICommandOwner owner;

        private ExecOwnedCommandHandler handler;

        public string CommandId => cmdId;

        public ICommandOwner Owner => owner;
        
        public OwnedCommand(ICommandOwner owner, ExecOwnedCommandHandler handler, string cmdId = null)
        {
            this.owner = owner;
            this.handler = handler;
            this.cmdId = cmdId ?? this.cmdId;
            if (owner != null) this.owner.PropertyChanged += Owner_PropertyChanged;
        }

        private void Owner_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == cmdId)
            //{
                QueryCanExecute();
            //}
        }

        public bool CanExecute(object parameter)
        {
            return canExecute;
        }

        public bool QueryCanExecute()
        {
            var exec = owner?.RequestCanExecute(cmdId) ?? false;

            if (exec != canExecute)
            {
                canExecute = exec;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            return exec;
        }

        public void Execute(object parameter)
        {
            handler?.Invoke(parameter);
        }
    }

    public interface IOwnedCommand : ICommand
    {
        ICommandOwner Owner { get; }

        bool QueryCanExecute();
    }

}

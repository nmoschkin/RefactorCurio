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


    public class OwnedCommand : ObservableBase, IOwnedCommand
    {
        public event EventHandler CanExecuteChanged;

        private string cmdId = Guid.NewGuid().ToString("d");

        private bool? canExecute = null;

        private ICommandOwner owner;

        private ExecOwnedCommandHandler handler;

        public string CommandId => cmdId;

        public bool? IsEnabled
        {
            get => canExecute;
            set
            {
                if (SetProperty(ref canExecute, value))
                {
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

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
            return canExecute ?? QueryCanExecute();
        }

        public bool QueryCanExecute()
        {
            var exec = owner?.RequestCanExecute(cmdId) ?? false;

            if (canExecute != null && exec != canExecute)
            {
                canExecute = exec;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged(nameof(IsEnabled));
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

        bool? IsEnabled { get; set; }

        bool QueryCanExecute();
    }

}

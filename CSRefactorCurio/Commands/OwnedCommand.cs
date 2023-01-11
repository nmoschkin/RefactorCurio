using DataTools.Essentials.Observable;

using System;
using System.Linq;
using System.Text;

namespace CSRefactorCurio
{
    internal delegate void ExecOwnedCommandHandler(object parameter);

    /// <summary>
    /// An implementation of the <see cref="IOwnedCommand"/> interface
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    internal class OwnedCommand : ObservableBase, IOwnedCommand
    {
        public event EventHandler CanExecuteChanged;

        private string cmdId = Guid.NewGuid().ToString("d");

        private bool? canExecute = null;

        private WeakReference<ICommandOwner> owner;

        private ExecOwnedCommandHandler handler;
        private bool disposedValue;

        public string CommandId => cmdId;

        /// <summary>
        /// Gets or sets a value indicating that this command is enabled.
        /// </summary>
        /// <remarks>
        /// Setting this value to false will cause <see cref="CanExecute(object)"/> to return false.<br/>
        /// Setting this value to null will cause <see cref="QueryCanExecute"/> to be called by <see cref="CanExecute(object)"/>.
        /// </remarks>
        public virtual bool? IsEnabled
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

        /// <summary>
        /// Gets the owner of the command.
        /// </summary>
        public virtual ICommandOwner Owner
        {
            get
            {
                ICommandOwner owner = null;
                this.owner?.TryGetTarget(out owner);
                return owner;
            }
            protected set
            {
                if (value == null)
                {
                    owner = null;
                }
                else
                {
                    owner = new WeakReference<ICommandOwner>(value);
                }
            }
        }

        public OwnedCommand(ICommandOwner owner, ExecOwnedCommandHandler handler, string cmdId = null)
        {
            Owner = owner;

            this.handler = handler;
            this.cmdId = cmdId ?? this.cmdId;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute ?? QueryCanExecute();
        }

        public bool QueryCanExecute()
        {
            var exec = Owner?.RequestCanExecute(cmdId) ?? false;

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                owner = null;
                handler = null;

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~OwnedCommand()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
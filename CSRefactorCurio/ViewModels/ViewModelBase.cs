using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace CSRefactorCurio.ViewModels
{
    /// <summary>
    /// This is a base view model that implements an observable pattern and also a command pattern.
    /// </summary>
    internal abstract class ViewModelBase : ICommandOwner, INotifyPropertyChanged
    {
        private readonly Dictionary<string, IOwnedCommand> registeredCommands = new Dictionary<string, IOwnedCommand>();

        protected static SynchronizationContext sync;

        protected static bool nopost;
        protected static bool nosend;

        private IOwnedCommand cancelCommand;
        private IOwnedCommand okCommand;
        private IOwnedCommand resetCommand;

        private bool changed;
        private string title;

        public virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fired when the user has requested that the window be closed.
        /// </summary>
        public virtual event EventHandler<RequestCloseEventArgs> RequestClose;

        #region Static and Synchronization Context

        static ViewModelBase()
        {
            sync = SynchronizationContext.Current;
            TestSendPost();
        }

        /// <summary>
        /// Call this method from the main thread to initialize the global UI context thread dispatcher.
        /// </summary>
        /// <param name="initSync"></param>
        public static void Initialize(SynchronizationContext initSync = null)
        {
            sync = initSync ?? SynchronizationContext.Current;
            TestSendPost();
        }

        /// <summary>
        /// Test the send or post ability of the synchronization object
        /// </summary>
        protected static void TestSendPost()
        {
            if (sync == null)
            {
                nosend = true;
                nopost = true;

                return;
            }

            try
            {
                sync.Post((o) => { int i = 0; }, null);
            }
            catch
            {
                nopost = true;
            }

            try
            {
                sync.Send((o) => { int i = 0; }, null);
            }
            catch
            {
                nosend = true;
            }
        }

        /// <summary>
        /// Begin invoking a method on the main thread, and return immediately.
        /// </summary>
        /// <param name="action">The action or lambda</param>
        protected void BeginInvoke(Action action) => BeginInvoke((o) => action(), null);

        /// <summary>
        /// Begin invoking a method on the main thread, and return immediately.
        /// </summary>
        /// <param name="action">The action or lambda that takes a single object as a parameter</param>
        /// <param name="obj">Parameter object (can be null)</param>
        protected void BeginInvoke(SendOrPostCallback action, object obj)
        {

            _ = Task.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                action(obj);
            });

            //if (nosend && nopost)
            //{
            //    _ = Task.Run(() => action(obj));
            //}
            //else if (nopost)
            //{
            //    _ = InvokeAsync(action, obj);
            //}
            //else
            //{
            //    sync.Post(action, obj);
            //}
        }

        /// <summary>
        /// Invoke the method on the main thread asynchronously
        /// </summary>
        /// <param name="action">The action or lambda</param>
        protected Task InvokeAsync(Action action) => InvokeAsync((o) => action(), null);

        /// <summary>
        /// Invoke the method on the main thread asynchronously
        /// </summary>
        /// <param name="action">The action or lambda that takes a single object as a parameter</param>
        /// <param name="obj">Parameter object (can be null)</param>
        protected Task InvokeAsync(SendOrPostCallback action, object obj)
        {
            if (nosend && nopost)
            {
                return Task.Run(() => action(obj));
            }
            else if (nosend)
            {
                BeginInvoke(action, obj);
                return Task.CompletedTask;
            }
            else
            {
                return Task.Run(() => sync.Send(action, obj));
            }
        }

        #endregion Static and Synchronization Context

        /// <summary>
        /// Initialize this view model with the basics
        /// </summary>
        /// <param name="hasOk">Has an OK button</param>
        /// <param name="hasCancel">Has a Cancel button</param>
        /// <param name="hasReset">Has a Reset/Revert button</param>
        /// <param name="autoRegisterCommands">True to automatically register the default commands in the base constructor.</param>
        protected ViewModelBase(bool hasOk, bool hasCancel, bool hasReset, bool autoRegisterCommands)
        {
            if (sync == null) Initialize();

            if (hasOk)
            {
                okCommand = new OwnedCommand(this, (o) =>
                {
                    if (OnOKPressed()) RequestClose?.Invoke(this, new RequestCloseEventArgs(true));
                }, nameof(OKCommand));
            }

            if (hasCancel)
            {
                cancelCommand = new OwnedCommand(this, (o) =>
                {
                    if (OnCancelPressed()) RequestClose?.Invoke(this, new RequestCloseEventArgs(true));
                }, nameof(CancelCommand));
            }

            if (hasReset)
            {
                resetCommand = new OwnedCommand(this, (o) =>
                {
                    if (OnResetPressed()) ResetForm();
                }, nameof(ResetCommand));
            }

            if (autoRegisterCommands) AutoRegisterCommands(this);
        }

        /// <summary>
        /// Gets or sets the Cancel Command
        /// </summary>
        public virtual IOwnedCommand CancelCommand
        {
            get => cancelCommand;
            set
            {
                if (SetProperty(ref cancelCommand, value))
                {
                    registeredCommands[nameof(CancelCommand)] = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating that data we are interested in, in this object has changed.
        /// </summary>
        public virtual bool Changed
        {
            get => changed;
            set
            {
                if (SetProperty(ref changed, value))
                {
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        /// <summary>
        /// Gets or sets the OK Command
        /// </summary>
        public virtual IOwnedCommand OKCommand
        {
            get => okCommand;
            set
            {
                if (SetProperty(ref okCommand, value))
                {
                    registeredCommands[nameof(OKCommand)] = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Reset Command
        /// </summary>
        public virtual IOwnedCommand ResetCommand
        {
            get => resetCommand;
            set
            {
                if (SetProperty(ref resetCommand, value))
                {
                    registeredCommands[nameof(ResetCommand)] = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a title string
        /// </summary>
        public virtual string Title
        {
            get => changed ? "* " + title : title;
            set
            {
                SetProperty(ref title, value);
            }
        }

        /// <summary>
        /// Accept the changes and set <see cref="Changed"/> to false.
        /// </summary>
        /// <remarks>
        /// Override this in your derived class to save your work.
        /// </remarks>
        public virtual void AcceptChanges()
        {
            Changed = false;
        }

        /// <summary>
        /// Reset the data and set <see cref="Changed"/> to false.
        /// </summary>
        /// <remarks>
        /// Override this in a derived class to discard your work.
        /// </remarks>
        public virtual void ResetForm()
        {
            Changed = false;
        }

        /// <summary>
        /// Override this method to do work or cancel a Reset command when the Reset button is pressed.
        /// </summary>
        /// <remarks>
        /// Return false to cancel the current sequence of activities.
        /// </remarks>
        protected virtual bool OnResetPressed()
        {
            return true;
        }

        /// <summary>
        /// Override this method to do work or cancel an OK command when the OK button is pressed.
        /// </summary>
        /// <remarks>
        /// Return false to cancel the current sequence of activities.
        /// </remarks>
        protected virtual bool OnOKPressed()
        {
            return true;
        }

        /// <summary>
        /// Override this method to do work or cancel a Cancel command when the Cancel button is pressed.
        /// </summary>
        /// <remarks>
        /// Return false to cancel the current sequence of activities.
        /// </remarks>
        protected virtual bool OnCancelPressed()
        {
            return true;
        }

        #region ICommandOwner

        /// <summary>
        /// Gets or sets the registered command with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IOwnedCommand this[string key]
        {
            get
            {
                if (registeredCommands.TryGetValue(key, out var command))
                {
                    return command;
                }

                return null;
            }
            set
            {
                if (registeredCommands.ContainsKey(key))
                {
                    registeredCommands[key] = value;
                }
                else
                {
                    registeredCommands.Add(key, value);
                }
            }
        }

        public void QueryAllCommands()
        {
            foreach (var kv in registeredCommands)
            {
                kv.Value.QueryCanExecute();
            }
        }

        public virtual bool RequestCanExecute(string commandId)
        {
            return true;
        }

        /// <summary>
        /// Automatically register all commands for the specified target using reflection.
        /// </summary>
        /// <param name="target">The target to initialize.</param>
        /// <returns>The number of commands registered.</returns>
        protected static int AutoRegisterCommands(ViewModelBase target)
        {
            var props = target.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            target.registeredCommands.Clear();

            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(IOwnedCommand) || prop.PropertyType.GetInterface(typeof(IOwnedCommand).FullName) != null)
                {
                    try
                    {
                        IOwnedCommand obj = prop.GetValue(target) as IOwnedCommand;
                        if (obj != null) target.registeredCommands.Add(prop.Name, obj);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

            return target.registeredCommands.Count;
        }

        #endregion ICommandOwner

        #region INotifyPropertyChanged

        /// <summary>
        /// Raise the <see cref="PropertyChanged"/> event (on the main thread, if possible)
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="noInvokeOnMainThread">Set to true to not attempt to invoke the property changed on main thread.</param>
        /// <remarks>
        /// Usually you want this event to fire on the main thread.<br />
        /// There is a much higher chance that the bound component will see the change.
        /// </remarks>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null, bool noInvokeOnMainThread = false)
        {
            if (!noInvokeOnMainThread)
            {
                BeginInvoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Set the target <paramref name="backingStore"/> to the <paramref name="value"/> and raise <see cref="PropertyChanged"/>, only if they are not equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="backingStore">The target.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="propertyName">The calling property.</param>
        /// <returns>True if the value was changed and the event was fired.</returns>
        /// <remarks>
        /// The default behavior is to call <see cref="Object.Equals(object)"/>, if neither object is null.
        /// </remarks>
        protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            bool eq;

            if (backingStore is null && value is null)
            {
                eq = true;
            }
            else if (backingStore is null || value is null)
            {
                eq = false;
            }
            else
            {
                eq = backingStore.Equals(value);
            }

            if (!eq)
            {
                backingStore = value;
                OnPropertyChanged(propertyName);
            }

            return !eq;
        }

        #endregion INotifyPropertyChanged

        #region IDisposable

        private bool disposedValue;

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Not used
                }

                foreach (var kv in registeredCommands)
                {
                    kv.Value?.Dispose();
                }

                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~ViewModelBase()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        #endregion IDisposable
    }
}
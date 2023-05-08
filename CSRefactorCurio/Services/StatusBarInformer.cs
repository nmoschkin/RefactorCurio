using DataTools.Essentials.Broadcasting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;

namespace CSRefactorCurio.Services
{
    /// <summary>
    /// Status Bar Informer Abstract Class
    /// </summary>
    /// <typeparam name="T">The type of object we're enumerating with the status bar</typeparam>
    /// <remarks>
    /// Subscription to the status bar dispatcher is required. Use this object in a using block for best results.<br /><br />
    /// Objects that are *not* locked for life may be locked, unlocked, and relocked, repeatedly.
    /// </remarks>
    internal abstract class StatusBarInformer<T> : Broadcaster<IStatusProgress> 
    {
        private ISubscription<IStatusProgress> subscription;

        private int count;
        private int pos;

        /// <summary>
        /// True if this object is locked for its lifetime
        /// </summary>
        /// <remarks>
        /// If this is true, wrap usage of derived classes in using blocks for best results.<br />
        /// Otherwise, the status bar lock may not be released unless it is garbage collected.
        /// </remarks>
        public bool LifetimeLock { get; private set; }


        public bool IsLocked => subscription != null && subscription.Validate();

        /// <summary>
        /// Gets the count of elements
        /// </summary>
        public virtual int Count
        {
            get => count;
            protected set
            {
                SetProperty(ref count, value);
            }
        }
        
        /// <summary>
        /// Gets the position in the counter
        /// </summary>
        public virtual int Position
        {
            get => pos;
            protected set
            {
                SetProperty(ref pos, value);
            }
        }

        /// <summary>
        /// Create a new status bar informer with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lifetimeLock">True to lock and hold the object for life.</param>
        /// <exception cref="InvalidOperationException"></exception>
        protected StatusBarInformer(string name, bool lifetimeLock) : base(InvocationType.Dispatcher, ChannelToken.CreateToken(name), name)
        {
            LifetimeLock = false;

            if (lifetimeLock)
            {
                LockStatusBar(true);
            }

            LifetimeLock = lifetimeLock;
        }

        /// <summary>
        /// Lock the status bar.
        /// </summary>
        /// <param name="throwOnFailLock">True to throw an error if it fails to acquire a lock.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <remarks>
        /// This method will throw an error if <paramref name="throwOnFailLock"/> is true only if it fails to acquire a lock.<br />
        /// If this object already has the lock, this function returns true.
        /// </remarks>
        protected bool LockStatusBar(bool throwOnFailLock)
        {
            if (LifetimeLock) return false;
            if (!IsLocked)
            {
                if (StatusBarDispatcherService.Instance.RequestLock(ChannelToken))
                {
                    subscription = Subscribe(StatusBarDispatcherService.Instance, ChannelToken);
                    return true;
                }
                else
                {
                    if (throwOnFailLock) throw new InvalidOperationException("Access Denied. Another object already has the status bar");
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Release the current lock.
        /// </summary>
        /// <returns>True if the lock was released or if the object was never locked.</returns>
        protected bool Release()
        {
            if (LifetimeLock) return false;

            if (subscription != null)
            {
                StatusBarDispatcherService.Instance.Unlock(ChannelToken);

                subscription.Dispose();
                subscription = null;

                return true;
            }

            return true;
        }

        /// <summary>
        /// Sets the count of elements
        /// </summary>
        /// <param name="newCount">The new number of elements.</param>
        /// <param name="resetPosition">True to reset the <see cref="Position"/> property to 0 (default is true.)</param>
        protected void SetCount(int newCount, bool resetPosition = true)
        {
            if (resetPosition) Position = 0;
            else if (Position > newCount) Position = newCount;
            Count = newCount;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (subscription != null)
                {
                    StatusBarDispatcherService.Instance.Unlock(ChannelToken);

                    subscription.Dispose();
                    subscription = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Inform from 0 to <paramref name="count"/>.
        /// </summary>
        /// <param name="count">The number of iterations to process</param>
        /// <param name="getText">The method used to get status bar text.</param>
        /// <param name="process">The method to do the work for the current element</param>
        /// <param name="resetCount">True to reset the <see cref="Count"/> and <see cref="Position"/> properties to the size of the specified data (default is true.)</param>
        protected virtual void Inform(int count, Func<int, int, string> getText, Action<int, int> process, bool resetCount = true)
        {
            try
            {
                if (!LifetimeLock) LockStatusBar(true);
                if (resetCount) SetCount(count);
                for (int i = 0; i < count; i++)
                {
                    Inform(true, getText(i, count));
                    process(i, count);
                }
            }
            finally
            {
                if (resetCount) Inform(false);
                if (!LifetimeLock) Release();
            }
        }

        /// <summary>
        /// Inform from 0 to <paramref name="count"/>.
        /// </summary>
        /// <param name="count">The number of iterations to process</param>
        /// <param name="process">The method to do the work for the current element and return status bar text</param>
        /// <param name="resetCount">True to reset the <see cref="Count"/> and <see cref="Position"/> properties to the size of the specified data (default is true.)</param>
        protected virtual void Inform(int count, Func<int, int, string> process, bool resetCount = true)
        {
            try
            {
                if (!LifetimeLock) LockStatusBar(true); 
                if (resetCount) SetCount(count);

                for (int i = 0; i < count; i++)
                {
                    Inform(true, process(i, count));
                }
            }
            finally
            {
                if (resetCount) Inform(false);
                if (!LifetimeLock) Release();
            }
        }

        /// <summary>
        /// Inform and do work on a series of data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="work"></param>
        /// <param name="resetCount">True to reset the <see cref="Count"/> and <see cref="Position"/> properties to the size of the specified data (default is true.)</param>
        protected virtual void Inform(IEnumerable<T> data, Action<T> work, bool resetCount = true)
        {
            try
            {
                if (!LifetimeLock) LockStatusBar(true);
             
                if (resetCount)
                {
                    int c;

                    if (data is IList<T> dlist)
                    {
                        c = dlist.Count;
                    }
                    else if (data is ICollection<T> dcol)
                    {
                        c = dcol.Count;
                    }
                    else
                    {
                        c = data.Count();
                    }

                    SetCount(c);
                }

                foreach (T item in data)
                {
                    Inform(item);
                    work(item);
                }
            }
            finally
            {
                if (resetCount) Inform(false);
                if (!LifetimeLock) Release();
            }
        }

        /// <summary>
        /// Get the status bar text for the current item
        /// </summary>
        /// <param name="item">Item</param>
        protected abstract string GetInformText(T item);

        protected void Inform(T data)
        {
            Inform(true, GetInformText(data));
        }

        /// <summary>
        /// Inform the status bar of a message
        /// </summary>
        /// <param name="running">True if the status is running, or false to end it</param>
        /// <param name="message">The message to put on the status bar</param>
        /// <param name="increment">True to increment the <see cref="Position"/> counter (default is true.)</param>
        protected void Inform(bool running, string message = "", bool increment = true)
        {
            if (running)
            {
                TransmitData(
                    new StatusProgress(
                        StatusProgressMode.Run, 
                        message, 
                        increment ? Position++ : Position, 
                        Count), 
                    InvocationType, 
                    ChannelToken);
            }
            else
            {
                Position = 0;
                TransmitData(
                    new StatusProgress(StatusProgressMode.EndGood), 
                    InvocationType, 
                    ChannelToken);
            }
        }
        
        /// <summary>
        /// End the status progress with an error message
        /// </summary>
        /// <param name="message"></param>
        protected virtual void Error(string message)
        {
            TransmitData(new StatusProgress(StatusProgressMode.EndError, message));
            if (!LifetimeLock) Release();
        }

        /// <summary>
        /// End the status progress with an error 
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void Error(Exception ex)
        {
            TransmitData(new StatusProgress(StatusProgressMode.EndError, ex.Message));
            if (!LifetimeLock) Release();
        }

        /// <summary>
        /// Abort the status progress with an optional abort message
        /// </summary>
        /// <param name="message"></param>
        protected virtual void Abort(string message = "")
        {
            TransmitData(new StatusProgress(StatusProgressMode.EndAbort, message));
            if (!LifetimeLock) Release();
        }
    }    
}

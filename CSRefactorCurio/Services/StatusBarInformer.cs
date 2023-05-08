using DataTools.Essentials.Broadcasting;
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
        
        /// <summary>
        /// True if this object is locked for its lifetime
        /// </summary>
        /// <remarks>
        /// If this is true, wrap usage of derived classes in using blocks for best results.<br />
        /// Otherwise, the status bar lock may not be released unless it is garbage collected.
        /// </remarks>
        public bool LifetimeLock { get; private set; }

        /// <summary>
        /// Create a new status bar informer with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lifetimeLock">True to lock and hold the object for life.</param>
        /// <exception cref="InvalidOperationException"></exception>
        protected StatusBarInformer(string name, bool lifetimeLock) : base(InvocationType.Dispatcher, ChannelToken.CreateToken(name), name)
        {
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
        /// If this method fails for other reasons, this method will return false.
        /// </remarks>
        protected bool LockStatusBar(bool throwOnFailLock)
        {
            if (LifetimeLock) return false;
            if (subscription == null || !subscription.Validate())
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
                return false;
            }
        }

        /// <summary>
        /// Release the current lock.
        /// </summary>
        /// <returns>True if the lock was released, false if this method fails for any reason.</returns>
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

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue && subscription != null)
            {
                StatusBarDispatcherService.Instance.Unlock(ChannelToken);

                subscription.Dispose();
                subscription = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Inform the status bar of the processing of data
        /// </summary>
        /// <param name="data">Data being processed</param>
        /// <param name="value">The current progress</param>
        /// <param name="count">The total count</param>
        protected abstract void Inform(T data, int value = 0, int count = 0);

        /// <summary>
        /// Inform the status bar of a message
        /// </summary>
        /// <param name="running">True if the status is running, or false to end it</param>
        /// <param name="message">The message to put on the status bar</param>
        /// <param name="value">The current progress</param>
        /// <param name="count">The total count</param>
        protected void Inform(bool running, string message = "", int value = 0, int count = 0)
        {
            if (running)
            {
                TransmitData(new StatusProgress(StatusProgressMode.Run, message, value, count), InvocationType, ChannelToken);
            }
            else
            {
                TransmitData(new StatusProgress(StatusProgressMode.EndGood), InvocationType, ChannelToken);
            }
        }
        
        /// <summary>
        /// End the status progress with an error message
        /// </summary>
        /// <param name="message"></param>
        protected virtual void Error(string message)
        {
            TransmitData(new StatusProgress(StatusProgressMode.EndError, message));
        }

        /// <summary>
        /// End the status progress with an error 
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void Error(Exception ex)
        {
            TransmitData(new StatusProgress(StatusProgressMode.EndError, ex.Message));
        }

        /// <summary>
        /// Abort the status progress with an optional abort message
        /// </summary>
        /// <param name="message"></param>
        protected virtual void Abort(string message = "")
        {
            TransmitData(new StatusProgress(StatusProgressMode.EndAbort, message));
        }
    }    
}

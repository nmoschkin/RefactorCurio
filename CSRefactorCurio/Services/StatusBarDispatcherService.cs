using DataTools.Essentials.Broadcasting;
using EnvDTE;
using System.Diagnostics;

namespace CSRefactorCurio.Services
{
    /// <summary>
    /// Represents the singleton status bar dispatcher service
    /// </summary>
    internal sealed class StatusBarDispatcherService : ISubscriber<IStatusProgress>
    {
        public static StatusBarDispatcherService Instance { get; private set; }

        public static bool Initialize(DTE dte)
        {
            try
            {
                Instance = new StatusBarDispatcherService(dte);
                return true;
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex);    
                return false;
            }
        }

        DTE _dte;
        ChannelToken lockToken = ChannelToken.Empty;

        public bool RequestLock(ChannelToken token)
        {
            if (lockToken != ChannelToken.Empty && lockToken != token) return false;
            lockToken = token;

            return true;
        }

        public bool Unlock(ChannelToken token)
        {
            if (lockToken == token)
            {
                lockToken = ChannelToken.Empty;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the token for the current object that is using the status bar.
        /// </summary>
        public bool IsLocked => lockToken != ChannelToken.Empty;

        /// <summary>
        /// Release the lock token forcibly
        /// </summary>
        /// <returns>True when completed</returns>
        public bool DangerousReleaseLock()
        {
            lockToken = ChannelToken.Empty;
            return true;
        }

        private StatusBarDispatcherService(DTE dte)
        {
            _dte = dte ?? throw new InvalidOperationException("Something is wrong with the plug-in");

        }

        public void ReceiveData(IStatusProgress value, ISideBandData sideBandData)
        {
            if (lockToken == ChannelToken.Empty || lockToken == sideBandData.ChannelToken)
            {
                if (value.Mode == StatusProgressMode.Initialize || value.Mode == StatusProgressMode.Run)
                {
                    _dte.StatusBar.Progress(true, value.Message, value.Value, value.Count);
                }
                else
                {
                    _dte.StatusBar.Progress(false, value.Message, value.Value, value.Count);
                }
            }
        }
    }
}

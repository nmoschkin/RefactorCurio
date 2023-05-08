namespace CSRefactorCurio.Services
{
    /// <summary>
    /// Status Bar Progress Class
    /// </summary>
    internal class StatusProgress : IStatusProgress
    {
        /// <summary>
        /// The run mode 
        /// </summary>
        public StatusProgressMode Mode { get; }

        /// <summary>
        /// The total count
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// The current progress
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// The message text to display on the status bar
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Create a new status bar progress
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="message"></param>
        /// <param name="value"></param>
        /// <param name="count"></param>
        public StatusProgress(StatusProgressMode mode, string message, int value, int count)
        {
            Mode = mode;
            Value = value;
            Message = message;
            Count = count;
        }

        /// <summary>
        /// Create a new status bar progress
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="message"></param>
        public StatusProgress(StatusProgressMode mode, string message)
        {
            Mode = mode;
            Value = 0;
            Message = message;
            Count = 0;
        }

        /// <summary>
        /// Create a new status bar progress
        /// </summary>
        /// <param name="mode"></param>
        public StatusProgress(StatusProgressMode mode)
        {
            Mode = mode;
            Value = 0;
            Message = "";
            Count = 0;
        }

    }
}

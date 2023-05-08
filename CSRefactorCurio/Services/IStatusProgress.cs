namespace CSRefactorCurio.Services
{
    /// <summary>
    /// Status Bar Progress Interface
    /// </summary>
    internal interface IStatusProgress
    {
        /// <summary>
        /// The run mode 
        /// </summary>
        StatusProgressMode Mode { get; }

        /// <summary>
        /// The total count
        /// </summary>
        int Count { get; }

        /// <summary>
        /// The current progress
        /// </summary>
        int Value { get; }

        /// <summary>
        /// The message text to display on the status bar
        /// </summary>
        string Message { get; }
    }
}

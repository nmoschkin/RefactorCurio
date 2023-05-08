namespace CSRefactorCurio.Services
{
    /// <summary>
    /// Status bar progress run modes
    /// </summary>
    internal enum StatusProgressMode
    {
        /// <summary>
        /// Initialize
        /// </summary>
        Initialize,

        /// <summary>
        /// Running
        /// </summary>
        Run,

        /// <summary>
        /// Ended Normally
        /// </summary>
        EndGood,

        /// <summary>
        /// Ended on Error
        /// </summary>
        EndError,

        /// <summary>
        /// User Aborted
        /// </summary>
        EndAbort
    }
}

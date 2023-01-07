namespace DataTools.Code.Filtering
{
    /// <summary>
    /// Filter Pass Mode
    /// </summary>
    internal enum FilterPassMode
    {
        /// <summary>
        /// All rules must pass for validity.
        /// </summary>
        PassAll,

        /// <summary>
        /// Any rule must pass for validity.
        /// </summary>
        PassAny,
    }
}
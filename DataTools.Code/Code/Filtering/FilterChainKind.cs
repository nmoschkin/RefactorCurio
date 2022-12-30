namespace DataTools.Code.Filtering
{
    /// <summary>
    /// Filter Chain Kinds
    /// </summary>
    internal enum FilterChainKind
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
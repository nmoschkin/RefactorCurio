namespace DataTools.CSTools
{
    /// <summary>
    /// Detected access modifiers
    /// </summary>
    [Flags]
    internal enum AccessModifiers
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Private
        /// </summary>
        Private = 0x1,

        /// <summary>
        /// Protected
        /// </summary>
        Protected = 0x2,

        /// <summary>
        /// Internal
        /// </summary>
        Internal = 0x4,

        /// <summary>
        /// Public
        /// </summary>
        Public = 0x8,

        /// <summary>
        /// Global
        /// </summary>
        Global = 0x10
    }
}
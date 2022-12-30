namespace DataTools.CSTools
{
    /// <summary>
    /// Project element types.
    /// </summary>
    [Flags]
    internal enum ElementType
    {
        /// <summary>
        /// Unknown/generic element type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Solution folder node.
        /// </summary>
        SolutionFolder = 0x01,

        /// <summary>
        /// Project node
        /// </summary>
        Project = 0x02,

        /// <summary>
        /// Project directory.
        /// </summary>
        Directory = 0x04,

        /// <summary>
        /// Namespace element.
        /// </summary>
        Namespace = 0x08,

        /// <summary>
        /// File or Document
        /// </summary>
        File = 0x10,

        /// <summary>
        /// Elements inside of a file or document.
        /// </summary>
        Marker = 0x20,

        /// <summary>
        /// This is a report node.
        /// </summary>
        ReportNode = 0x40,

        /// <summary>
        /// This is a project view layout.
        /// </summary>
        ProjectView = 0x80,

        Any = 0xff
    }
}
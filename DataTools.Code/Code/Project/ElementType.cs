using System;

namespace DataTools.Code.Project
{
    /// <summary>
    /// Project element types.
    /// </summary>
    [Flags]
    public enum ElementType
    {
        /// <summary>
        /// Unknown/generic element type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Solution.
        /// </summary>
        Solution = 0x01,

        /// <summary>
        /// Solution folder node.
        /// </summary>
        SolutionFolder = 0x02,

        /// <summary>
        /// Project node
        /// </summary>
        Project = 0x04,

        /// <summary>
        /// Project directory.
        /// </summary>
        Directory = 0x08,

        /// <summary>
        /// Namespace element.
        /// </summary>
        Namespace = 0x10,

        /// <summary>
        /// File or Document
        /// </summary>
        File = 0x20,

        /// <summary>
        /// Elements inside of a file or document.
        /// </summary>
        Marker = 0x40,

        /// <summary>
        /// This is a report node.
        /// </summary>
        ReportNode = 0x80,

        /// <summary>
        /// This is a project view layout.
        /// </summary>
        ProjectView = 0x100,

        /// <summary>
        /// Any project item is permissible or expected.
        /// </summary>
        Any = 0xfff
    }
}
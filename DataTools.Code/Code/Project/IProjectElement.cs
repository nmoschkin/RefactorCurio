using System;

namespace DataTools.Code.Project
{
    /// <summary>
    /// Represents a basic solution element.
    /// </summary>
    internal interface ISolutionElement
    {
        /// <summary>
        /// The element type.
        /// </summary>
        ElementType ElementType { get; }

        /// <summary>
        /// The title of the element.
        /// </summary>
        string Title { get; }
    }

    /// <summary>
    /// Represents a basic project element.
    /// </summary>
    internal interface IProjectElement : ISolutionElement
    {
        /// <summary>
        /// Gets the parent element, if any.
        /// </summary>
        ISolutionElement ParentElement { get; }
    }
}
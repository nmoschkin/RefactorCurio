using System;

namespace DataTools.Code.Project
{
    /// <summary>
    /// Represents the basis for all solution elements.
    /// </summary>
    public interface ISolutionElement
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
    /// Represents the basis for all project elements.
    /// </summary>
    public interface IProjectElement : ISolutionElement
    {
        /// <summary>
        /// Gets the parent element (if any.)
        /// </summary>
        ISolutionElement ParentElement { get; }
    }
}
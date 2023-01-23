using System;

namespace DataTools.Code.Project
{
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
using System.Collections.Generic;

namespace DataTools.Code.Project
{
    /// <summary>
    /// Represents the solution object, which is the top-level element of the project hierarchy.
    /// </summary>
    /// <remarks>
    /// A <see cref="ISolution"/> object is also an <see cref="ISolutionElement"/>, as are all elements contained, therein.
    /// </remarks>
    public interface ISolution : ISolutionElement
    {
        IList<IProjectElement> Namespaces { get; }

        IList<IProjectElement> Projects { get; }
    }
}
using System.Collections.Generic;

namespace DataTools.Code.Project
{
    internal interface ISolution : ISolutionElement
    {
        IList<IProjectElement> Namespaces { get; }

        IList<IProjectElement> Projects { get; }
    }
}
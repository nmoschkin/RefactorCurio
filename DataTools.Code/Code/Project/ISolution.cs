using System.Collections.Generic;

namespace DataTools.Code.Project
{
    internal interface ISolution
    {
        IList<IProjectElement> Namespaces { get; }

        IList<IProjectElement> Projects { get; }
    }
}
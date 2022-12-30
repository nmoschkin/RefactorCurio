using System.Collections.Generic;

namespace DataTools.CSTools
{
    internal interface ISolution
    {
        IList<IProjectElement> Namespaces { get; }

        IList<IProjectElement> Projects { get; }
    }
}
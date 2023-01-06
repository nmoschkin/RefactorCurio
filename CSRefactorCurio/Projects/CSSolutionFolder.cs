using DataTools.Code.Project;

using System.Collections.ObjectModel;

namespace DataTools.CSTools
{
    /// <summary>
    /// Represents a solution folder in a project.
    /// </summary>
    internal class CSSolutionFolder : ProjectNodeBase<ObservableCollection<IProjectElement>>
    {
        public CSSolutionFolder(string title, ISolutionElement parent = null) : base(parent)
        {
            this.title = title;
        }

        public override ElementType ChildType => ElementType.Project | ElementType.SolutionFolder;
        public override ElementType ElementType => ElementType.SolutionFolder;
    }
}
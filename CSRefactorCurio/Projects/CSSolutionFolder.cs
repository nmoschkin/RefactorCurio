using DataTools.Code.Project;
using DataTools.Essentials.Observable;

using System.Collections;
using System.Collections.ObjectModel;

namespace DataTools.CSTools
{
    /// <summary>
    /// Represents a solution folder in a project.
    /// </summary>
    internal class CSSolutionFolder : ObservableBase, IProjectNode<ObservableCollection<IProjectElement>>
    {
        private ObservableCollection<IProjectElement> children = new ObservableCollection<IProjectElement>();
        private string title;

        public CSSolutionFolder(string title)
        {
            this.title = title;
        }

        IEnumerable IProjectNode.Children => Children;

        /// <summary>
        /// Gets the child project elements.
        /// </summary>
        public ObservableCollection<IProjectElement> Children
        {
            get => children;
            protected set
            {
                SetProperty(ref children, value);
            }
        }

        public ElementType ChildType => ElementType.Project;
        public ElementType ElementType => ElementType.SolutionFolder;

        public string Title
        {
            get => title;
            protected set
            {
                SetProperty(ref title, value);
            }
        }
    }
}
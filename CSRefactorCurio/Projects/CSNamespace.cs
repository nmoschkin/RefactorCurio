using DataTools.Code.Project;
using DataTools.Essentials.SortedLists;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DataTools.CSTools
{
    /// <summary>
    /// A CS Refactor Curio Solution Namespace
    /// </summary>
    internal class CSNamespace : ProjectNodeBase<ObservableCollection<IProjectElement>>, INamespace
    {
        private ObservableCollection<CSMarker> markers = new ObservableCollection<CSMarker>();
        private string name;
        private ObservableCollection<CSNamespace> namespaces = new ObservableCollection<CSNamespace>();

        /// <summary>
        /// Create a new namespace with the specified name and optional parent.
        /// </summary>
        /// <param name="name">Fully qualified namespace.</param>
        /// <param name="parent">Parent</param>
        public CSNamespace(string name, CSNamespace parent = null) : base(parent)
        {
            this.name = name;
        }

        /// <summary>
        /// Create a new namespace with the specified name, initial children and optional parent.
        /// </summary>
        /// <param name="items">The initial children.</param>
        /// <param name="name">Fully qualified namespace.</param>
        /// <param name="parent">Parent</param>
        public CSNamespace(string name, IEnumerable<IProjectElement> items, CSNamespace parent = null) : base(parent, items)
        {
            this.name = name;
        }

        string INamespace.Namespace
        {
            get => name;
            set
            {
                if (SetProperty(ref name, value))
                {
                    OnPropertyChanged(nameof(FullyQualifiedName));
                }
            }
        }

        public string FullyQualifiedName => name;

        public override ElementType ChildType => ElementType.SolutionFolder | ElementType.Namespace;

        public override ElementType ElementType => ElementType.Namespace;

        /// <summary>
        /// True if this is the root.
        /// </summary>
        public bool IsRoot => ParentElement == null;

        /// <summary>
        /// Gets the observable collection of code elements that are at home in this namespace.
        /// </summary>
        public ObservableCollection<CSMarker> Markers
        {
            get => markers;
            protected set
            {
                SetProperty(ref markers, value);
            }
        }

        /// <summary>
        /// Gets the namespace name.
        /// </summary>
        public string Name
        {
            get => name;
            protected set
            {
                SetProperty(ref name, value);
            }
        }

        /// <summary>
        /// Gets the observable collection of child <see cref="CSNamespace"/> objects.
        /// </summary>
        public ObservableCollection<CSNamespace> Namespaces
        {
            get => namespaces;
            protected set
            {
                SetProperty(ref namespaces, value);
            }
        }

        /// <summary>
        /// Gets the parent namespace, or null if this is a root namespace.
        /// </summary>
        public new CSNamespace ParentElement => (base.ParentElement as CSNamespace);

        /// <summary>
        /// Sort the namespace map in place.
        /// </summary>
        /// <param name="descending">Sort in descending order.</param>
        public void Sort(bool descending = false)
        {
            var m = descending ? -1 : 1;

            QuickSort.Sort(Children, (a, b) =>
            {
                if (a.ElementType == ElementType.Namespace && b.ElementType != ElementType.Namespace) return -1 * m;
                else if (a.ElementType != ElementType.Namespace && b.ElementType == ElementType.Namespace) return 1 * m;
                else return string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase);
            });

            QuickSort.Sort(Markers, (a, b) =>
            {
                return string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase);
            });

            QuickSort.Sort(Namespaces, (a, b) =>
            {
                return string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase);
            });
        }

        public override string ToString()
        {
            return Name;
        }

        
    }
}
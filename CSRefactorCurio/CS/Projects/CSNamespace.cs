using DataTools.CSTools;
using DataTools.Observable;
using DataTools.SortedLists;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTools.CSTools
{
    /// <summary>
    /// A CS Refactor Curio Solution Namespace
    /// </summary>
    public class CSNamespace : ObservableBase, IProjectNode<ObservableCollection<IProjectElement>>, INamespace
    {
        #region Private Fields

        private ObservableCollection<IProjectElement> children = new ObservableCollection<IProjectElement>();
        private ObservableCollection<CSMarker> markers = new ObservableCollection<CSMarker>();
        private string name;
        private ObservableCollection<CSNamespace> namespaces = new ObservableCollection<CSNamespace>();

        private CSNamespace parent;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Create a new namespace with the specified name and optional parent.
        /// </summary>
        /// <param name="name">Fully qualified namespace.</param>
        /// <param name="parent">Parent</param>
        public CSNamespace(string name, CSNamespace parent = null)
        {
            this.name = name;
            this.parent = parent;
        }

        /// <summary>
        /// Create a new namespace with the specified name, initial children and optional parent.
        /// </summary>
        /// <param name="items">The initial children.</param>
        /// <param name="name">Fully qualified namespace.</param>
        /// <param name="parent">Parent</param>
        public CSNamespace(string name, IEnumerable<IProjectElement> items, CSNamespace parent = null) : this(name, parent)
        {
            foreach (var item in items)
            {
                children.Add(item);
            }
        }

        #endregion Public Constructors

        #region Public Properties

        IList IProjectNode.Children => children;

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

        /// <summary>
        /// Gets the child objects (either namespaces or markers).
        /// </summary>
        public ObservableCollection<IProjectElement> Children
        {
            get => children;
            protected set
            {
                SetProperty(ref children, value);
            }
        }

        public ElementType ChildType => ElementType.SolutionFolder | ElementType.Namespace;

        public ElementType ElementType => ElementType.Namespace;

        /// <summary>
        /// True if this is the root.
        /// </summary>
        public bool IsRoot => parent == null;

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
        public CSNamespace Parent => parent;

        public string Title => name;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Generate a namespace map from the specified project.
        /// </summary>
        /// <param name="project">The project to generate the namespace map for.</param>
        /// <param name="namespaces">Optional namespace dictionary.</param>
        /// <returns>A new <see cref="ObservableCollection{T}"/> of <see cref="IProjectElement"/> objects.</returns>
        public static ObservableCollection<IProjectElement> NamespacesFromProject(CurioProject project, Dictionary<string, CSNamespace> namespaces = null)
        {
            return NamespacesFromProjects(new[] { project }, namespaces);
        }


        /// <summary>
        /// Generate a namespace map from the specified projects.
        /// </summary>
        /// <param name="projects">The projects to generate the namespace map for.</param>
        /// <param name="namespaces">Optional namespace dictionary.</param>
        /// <returns>A new <see cref="ObservableCollection{T}"/> of <see cref="IProjectElement"/> objects.</returns>
        public static ObservableCollection<IProjectElement> NamespacesFromProjects(IEnumerable<CurioProject> projects, Dictionary<string, CSNamespace> namespaces = null, EnvDTE.StatusBar statusBar = null)
        {
            namespaces ??= new Dictionary<string, CSNamespace>();

            int tc = 0;

            foreach (var project in projects)
            {
                var root = project.RootFolder;
                tc += root.GetTotalFilesCount();
            }

            int proc = 0;
            
            foreach (var project in projects)
            {
                var root = project.RootFolder;
                proc = NamespacesFromNode(root, namespaces, project.DefaultNamespace ?? project.AssemblyName ?? project.Title, statusBar, tc, proc); ;
            }

            if (statusBar != null)
            {
                statusBar.Progress(false);
            }

            return new ObservableCollection<IProjectElement>(namespaces.Values.Where((v) => v.Parent == null));
        }

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

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Ensure that a dictionary map of namespaces has the specified namespace element, creating it if necessary.
        /// </summary>
        /// <param name="name">The fully qualified namespace.</param>
        /// <param name="namespaces">The namespace map.</param>
        /// <returns>The found/created namespace.</returns>
        protected static CSNamespace EnsureNamespace(string name, Dictionary<string, CSNamespace> namespaces, string defaultNamespace)
        {
            name ??= defaultNamespace;

            var ns = name.Split('.');
            int c = ns.Length;
            var sb = new StringBuilder();

            CSNamespace lvn = null;

            for (int x = 0; x < c; x++)
            {
                if (sb.Length > 0) sb.Append('.');
                sb.Append(ns[x]);

                if (!namespaces.TryGetValue(sb.ToString(), out CSNamespace vns))
                {
                    vns = new CSNamespace(sb.ToString(), lvn);
                    if (lvn != null)
                    {
                        lvn.Children.Add(vns);
                        lvn.Namespaces.Add(vns);

                        lvn.Sort();
                    }

                    namespaces.Add(sb.ToString(), vns);
                }
        
                lvn = vns;
            }

            return lvn;
        }
        /// <summary>
        /// Get the namespaces for the specified node.
        /// </summary>
        /// <param name="node">The directory/node to map.</param>
        /// <param name="namespaces">The current namespace map.</param>
        protected static int NamespacesFromNode(CSDirectory node, Dictionary<string, CSNamespace> namespaces, string defaultNamespace, EnvDTE.StatusBar statusBar = null, int totalFiles = 0, int processed = 0)
        {
            foreach (var file in node.Files) 
            {
                processed++;
                if (statusBar != null)
                {
                    statusBar.Progress(true, file.Filename, processed, totalFiles);
                }

                foreach (CSMarker cls in file.FilteredItems)
                {
                    var ns = EnsureNamespace(cls.Namespace, namespaces, defaultNamespace);

                    if (ns != null)
                    {
                        ns.Children.Add(cls);
                        ns.Markers.Add(cls);

                        ns.Sort();
                    }
                }
            }

            foreach (var dir in node.Directories)
            {
                processed = NamespacesFromNode(dir, namespaces, defaultNamespace, statusBar, totalFiles, processed);
            }

            return processed;
        }

        #endregion Protected Methods
    }
}

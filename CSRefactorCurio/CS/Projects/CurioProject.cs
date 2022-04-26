
using CSRefactorCurio;

using DataTools.Desktop;
using DataTools.Observable;
using DataTools.Text;

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;

namespace DataTools.CSTools
{

    /// <summary>
    /// Project element types.
    /// </summary>
    [Flags]
    public enum ElementType
    {
        
        /// <summary>
        /// Unknown/generic element type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Solution folder node.
        /// </summary>
        SolutionFolder = 0x01,

        /// <summary>
        /// Project node
        /// </summary>
        Project = 0x02,

        /// <summary>
        /// Project directory.
        /// </summary>
        Directory = 0x04,

        /// <summary>
        /// Namespace element.
        /// </summary>
        Namespace = 0x08,

        /// <summary>
        /// File or Document
        /// </summary>
        File = 0x10,

        /// <summary>
        /// Elements inside of a file or document.
        /// </summary>
        Marker = 0x20,

        /// <summary>
        /// This is a report node.
        /// </summary>
        ReportNode = 0x40,


        /// <summary>
        /// This is a project view layout.
        /// </summary>
        ProjectView = 0x80,


        Any = 0xff
    }

    public interface INamespace : IProjectNode
    {
        /// <summary>
        /// Home namespace of this element.
        /// </summary>
        string Namespace { get; set; }

        /// <summary>
        /// Gets the fully-qualified name calculated from the <see cref="Namespace"/> and <see cref="Name"/> properties.
        /// </summary>
        string FullyQualifiedName { get; }

    }

    /// <summary>
    /// Represents a basic project element.
    /// </summary>
    public interface IProjectElement 
    {
        #region Public Properties

        /// <summary>
        /// The element type.
        /// </summary>
        ElementType ElementType { get; }

        /// <summary>
        /// The title of the element.
        /// </summary>
        string Title { get; }

        #endregion Public Properties
    }

    /// <summary>
    /// Represents a top-level project host element.
    /// </summary>
    public interface IProjectHost : IProjectElement, INotifyPropertyChanged
    {
        #region Public Properties

        /// <summary>
        /// The project properties.
        /// </summary>
        IPropertiesContainer Properties { get; }

        /// <summary>
        /// The root folder of the project children.
        /// </summary>
        IProjectNode RootFolder { get; }

        #endregion Public Properties
    }

    public interface ISolution
    {
        IList<IProjectElement> Namespaces { get; }

        IList<IProjectElement> Projects { get; }
    }

    /// <summary>
    /// Represents a project element with child elements.
    /// </summary>
    public interface IProjectNode : IProjectElement
    {
        #region Public Properties

        /// <summary>
        /// The element children.
        /// </summary>
        IList Children { get; }

        /// <summary>
        /// The type flags for the element children (can be or'd)
        /// </summary>
        ElementType ChildType { get; }

        #endregion Public Properties
    }

    /// <summary>
    /// Represents a project element with child elements that can be observed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProjectNode<T> : IProjectNode where T : IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Public Properties

        /// <summary>
        /// Gets the list of child nodes.
        /// </summary>
        new T Children { get; }

        #endregion Public Properties
    }

    /// <summary>
    /// Represents a project element with child elements based on a file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProjectFile : IProjectNode
    {
        string Filename { get; }

        string Text { get; }

    }

    public interface IProjectFile<T> : IProjectFile, IProjectNode<T> where T : IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }




    /// <summary>
    /// Represents a solution folder in a project.
    /// </summary>
    public class CSSolutionFolder : ObservableBase, IProjectNode<ObservableCollection<IProjectElement>>
    {
        #region Private Fields

        private ObservableCollection<IProjectElement> children = new ObservableCollection<IProjectElement>();
        private string title;

        #endregion Private Fields

        #region Public Constructors

        public CSSolutionFolder(string title)
        {
            this.title = title;
        }

        #endregion Public Constructors

        #region Public Properties

        IList IProjectNode.Children => Children;
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

        #endregion Public Properties
    }

    /// <summary>
    /// CS Refactor Curio Project Class
    /// </summary>
    public class CurioProject : ObservableBase, IProjectHost, IDisposable
    {
        #region Protected Fields

        protected bool disposedValue;

        #endregion Protected Fields

        #region Private Fields

        private EnvDTE.Project _project;
        private List<string> allNamespaces = new List<string>();
        private string assyname;
        private string defns;
        private List<string> excludes;
        private List<string> includes;
        private bool isFrameworkProject;
        private FSMonitor monitor;
        private string projectFile = "";
        private PropertiesContainer properties;
        private CSDirectory rootFolder = null;
        private string rootPath = "";
        private object selectedItem = null;
        private string title = null;
        private XmlDocument xml;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Create a new CS Refactor Curio Project from the specified project file and native COM object.
        /// </summary>
        /// <param name="filename">The project file to load.</param>
        /// <param name="nativeProject">The COM object from the DTE.</param>
        /// <exception cref="FileNotFoundException">If the project cannot be loaded.</exception>
        public CurioProject(string filename, EnvDTE.Project nativeProject)
        {
            _project = nativeProject;
            PopulateProjectProperties();
            
            if (filename == null || !File.Exists(filename)) throw new FileNotFoundException();

            ProjectRootPath = Path.GetFullPath(Path.GetDirectoryName(filename) ?? "");
            ProjectFile = Path.GetFileName(filename);

            monitor = new FSMonitor(ProjectRootPath, nativeProject.DTE.MainWindow.HWnd);

            monitor.WatchNotifyChange += OnDirectoryChanged;

            ReloadProject();
            monitor.Watch();
        }

        #endregion Public Constructors

        #region Private Destructors

        ~CurioProject()
        {
            monitor?.Dispose();
        }

        #endregion Private Destructors

        #region Public Properties

        /// <summary>
        /// Gets the assembly name for the project.
        /// </summary>
        public string AssemblyName => assyname;

        /// <summary>
        /// Gets the default namespace for the project.
        /// </summary>
        public string DefaultNamespace => defns;

        public ElementType ElementType => ElementType.Project;

        /// <summary>
        /// List of explicitly excluded files.
        /// </summary>
        public IReadOnlyList<string> Excludes => excludes;

        /// <summary>
        /// List of explicitly included files.
        /// </summary>
        public IReadOnlyList<string> Includes => includes;

        /// <summary>
        /// Gets a value indicating if the project is a .NET Framework project (as opposed to .NET Core/5/6/etc.)
        /// </summary>
        public bool IsFrameworkProject => isFrameworkProject;
        /// <summary>
        /// Gets a list of all namespace that are currently detected in the project.
        /// </summary>
        public IReadOnlyList<string> Namespaces => allNamespaces;

        /// <summary>
        /// Gets the native project COM object.
        /// </summary>
        public EnvDTE.Project NativeProject
        {
            get => _project;
        }

        /// <summary>
        /// Gets the name of the project file.
        /// </summary>
        public string ProjectFile
        {
            get => projectFile;
            protected set
            {
                if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
                if (SetProperty(ref projectFile, value))
                {
                    Title = Path.GetFileNameWithoutExtension(value);
                }
            }
        }

        /// <summary>
        /// Gets the full project root directory path.
        /// </summary>
        public string ProjectRootPath
        {
            get => rootPath;
            protected set
            {
                if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
                SetProperty(ref rootPath, value);
            }
        }

        public IPropertiesContainer Properties => properties;
        /// <summary>
        /// Gets the root folder of the project.
        /// </summary>
        public CSDirectory RootFolder
        {
            get => rootFolder;
            protected set
            {
                if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
                if (SetProperty(ref rootFolder, value))
                {
                    OnPropertyChanged(nameof(RootFolder));
                }
            }
        }

        IProjectNode IProjectHost.RootFolder => rootFolder;

        /// <summary>
        /// Gets or sets the currently selected item.
        /// </summary>
        public object SelectedItem
        {
            get => selectedItem;
            set
            {
                if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
                SetProperty(ref selectedItem, value);
            }
        }
        public string Title
        {
            get => title;
            protected set
            {
                if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
                SetProperty(ref title, value);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public virtual void Dispose()
        {
            ((IDisposable)monitor).Dispose();
            monitor = null;
            disposedValue = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Reload the project and all project files (Costly)
        /// </summary>
        public void ReloadProject()
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            xml = new XmlDocument();
            xml.LoadXml(File.ReadAllText($"{ProjectRootPath}\\{ProjectFile}"));

            bool isframe = false;

            var frame = xml.GetElementsByTagName("TargetFrameworkVersion");
            if (frame != null && frame.Count > 0)
            {
                isframe = true;
            }

            var compiles = xml.GetElementsByTagName("Compile");

            var incs = new List<string>();
            var excs = new List<string>();

            foreach (XmlNode compile in compiles)
            {
                if (isframe)
                {
                    if (compile.Attributes["Include"] is XmlAttribute xa)
                    {
                        incs.Add(xa.InnerText);
                    }
                }
                else
                {
                    if (compile.Attributes["Remove"] is XmlAttribute xa)
                    {
                        excs.Add(xa.InnerText);
                    }
                }
            }

            includes = incs;
            excludes = excs;
            isFrameworkProject = isframe;

            RootFolder = new CSDirectory(this, ProjectRootPath);

            allNamespaces.Clear();
            allNamespaces.Add(defns);
            allNamespaces.AddRange(RootFolder.GetAllNamespacesFromHere());
            allNamespaces = allNamespaces.Distinct().ToList();
            allNamespaces.Sort();

            OnPropertyChanged(nameof(Namespaces));
        }


        public override string ToString()
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);

            return Title;
        }

        #endregion Public Methods

        #region Protected Methods

        internal void ReadTheFile()
        {
            var path = ProjectRootPath + "\\bin\\Debug";
            var files = new List<string>(Directory.GetFiles(path, Title + ".dll"));
            var dirs = Directory.GetDirectories(path);

            if (files.Count == 0)
            {
                foreach (var dir in dirs)
                {
                    var f = Directory.GetFiles(dir, Title + ".dll");
                    if (f.Length > 0)
                    {
                        files.AddRange(f);
                        break;
                    }
                }
            }

            var file = files.FirstOrDefault();

            


        }

        /// <summary>
        /// Raised by the directory watcher to indicate that contents of the folder have changed.
        /// </summary>
        /// <remarks>
        /// The default behavior is to respond to individual changes and only refresh the entire project if the project file changes.
        /// </remarks>
        protected virtual void OnDirectoryChanged(object sender, FSMonitorEventArgs e)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            var efn = ProjectRootPath + "\\" + e.Info.Filename.ToLower();
            if (efn.EndsWith(".csproj"))
            {
                ReloadProject();
            }
            else if (efn.EndsWith(".cs") || (efn.Contains(".cs") && efn.Contains("TMP")))
            {
                RootFolder.ProcessChangeEvent(e.Info);
            }
        }

        /// <summary>
        /// Populate the properties container with the project properties from the native COM object.
        /// </summary>
        protected virtual void PopulateProjectProperties()
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            properties = new PropertiesContainer(_project);

            if (properties.ContainsKey("DefaultNamespace"))
            {
                defns = (string)properties["DefaultNamespace"].Value;
            }

            if (properties.ContainsKey("AssemblyName"))
            {
                assyname = (string)properties["AssemblyName"].Value;
            }
        }

        #endregion Protected Methods
    }


}

using CSRefactorCurio;
using CSRefactorCurio.Projects;

using DataTools.Code.Project;
using DataTools.Code.Project.Properties;
using DataTools.Desktop;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace DataTools.CSTools
{
    /// <summary>
    /// CS Refactor Curio Project Class
    /// </summary>
    internal class CurioProject : ProjectElementBase, IProjectHost, IDisposable
    {
        protected bool disposedValue;

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
        private XmlDocument xml;

        /// <summary>
        /// Create a new CS Refactor Curio Project from the specified project file and native COM object.
        /// </summary>
        /// <param name="filename">The project file to load.</param>
        /// <param name="nativeProject">The COM object from the DTE.</param>
        /// <exception cref="FileNotFoundException">If the project cannot be loaded.</exception>
        public CurioProject(string filename, EnvDTE.Project nativeProject, CurioExplorerSolution parent = null) : base(parent)
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

        /// <summary>
        /// Gets the assembly name for the project.
        /// </summary>
        public string AssemblyName => assyname;

        /// <summary>
        /// Gets the default namespace for the project.
        /// </summary>
        public string DefaultNamespace => defns;

        public override ElementType ElementType => ElementType.Project;

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

        public override string Title
        {
            get => title;
            protected set
            {
                if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
                SetProperty(ref title, value);
            }
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                //if (disposing)
                //{
                //    // Nothing to do in this section.
                //}

                ((IDisposable)monitor).Dispose();
                monitor = null;

                disposedValue = true;
            }
        }

        ~CurioProject()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
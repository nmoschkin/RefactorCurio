
using CSRefactorCurio;

using DataTools.Desktop;
using DataTools.Observable;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;

namespace DataTools.CSTools
{

    public enum ElementType
    {
        SolutionFolder,
        Project,
        Directory,
        Namespace,
        File,
        Marker
    }


    public interface IProjectElement
    {
        string Title { get; }

        ElementType ElementType { get; }

    }

    public interface IProjectNode : IProjectElement
    {
        ObservableCollection<IProjectElement> Children { get; }
    }

    public interface IProjectHost : IProjectElement
    {
        IProjectNode RootFolder { get; }

        IPropertiesContainer Properties { get; }
    }

    public class CSSolutionFolder : ObservableBase, IProjectNode
    {
        private string title;
        
        private ObservableCollection<IProjectElement> children = new ObservableCollection<IProjectElement>();
        
        public CSSolutionFolder(string title)
        {
            this.title = title;
        }

        public ObservableCollection<IProjectElement> Children
        {
            get => children;
            protected set
            {
                SetProperty(ref children, value);
            }
        }

        public string Title
        {
            get => title;
            protected set
            {
                SetProperty(ref title, value);
            }
        }

        public ElementType ElementType => ElementType.SolutionFolder;
    }

    public class CurioProject : ObservableBase, IProjectHost, IDisposable
    {
        private string projectRoot = "";
        private string projectFile = "";

        private EnvDTE.Project _project;
        private PropertiesContainer properties;
        private FSMonitor monitor;

        private CSDirectory rootFolder = null;

        private object selectedItem = null;
        private bool isFrameworkProject;

        private XmlDocument xml;
        private string title = null;

        private List<string> allNamespaces = new List<string>();
        private string defns;
        private string assyname;

        private List<string> includes;
        private List<string> excludes;

        public bool IsFrameworkProject => isFrameworkProject;

        public string DefaultNamespace => defns;

        public string AssemblyName => assyname;

        public IPropertiesContainer Properties => properties;

        public EnvDTE.Project NativeProject
        {
            get => _project;
        }

        public object SelectedItem
        {
            get => selectedItem;
            set
            {
                SetProperty(ref selectedItem, value);
            }
        }

        public CSDirectory RootFolder
        {
            get => rootFolder;
            protected set
            {
                if (SetProperty(ref rootFolder, value))
                {
                    OnPropertyChanged(nameof(RootFolder));
                }
            }
        }

        public IReadOnlyList<string> Includes => includes;

        public IReadOnlyList<string> Excludes => excludes;

        IProjectNode IProjectHost.RootFolder => rootFolder;

        public string ProjectRoot
        {
            get => projectRoot;
            protected set
            {
                SetProperty(ref projectRoot, value);
            }
        }

        public string ProjectFile
        {
            get => projectFile;
            protected set
            {
                if (SetProperty(ref projectFile, value))
                {
                    Title = Path.GetFileNameWithoutExtension(value);
                }
            }
        }

        public string Title
        {
            get => title;
            protected set
            {
                SetProperty(ref title, value);
            }
        }

        public List<string> Namespaces => allNamespaces;

        public void ReloadProject()
        {
            xml = new XmlDocument();
            xml.LoadXml(File.ReadAllText($"{ProjectRoot}\\{ProjectFile}"));

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

            RootFolder = new CSDirectory(this, ProjectRoot);

            allNamespaces.Clear();
            allNamespaces.Add(defns);
            allNamespaces.AddRange(RootFolder.GetAllNamespacesFromHere());
            allNamespaces = allNamespaces.Distinct().ToList();
            allNamespaces.Sort();

            OnPropertyChanged(nameof(Namespaces));
        }

        private void PopulateProjectProperties()
        {
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

        public CurioProject(string filename, EnvDTE.Project nativeProject)
        {
            _project = nativeProject;
            PopulateProjectProperties();

            if (filename == null || !File.Exists(filename)) throw new FileNotFoundException();

            ProjectRoot = Path.GetFullPath(Path.GetDirectoryName(filename) ?? "");
            ProjectFile = Path.GetFileName(filename);
            
            monitor = new FSMonitor(ProjectRoot, nativeProject.DTE.MainWindow.HWnd);

            monitor.WatchNotifyChange += Monitor_WatchNotifyChange;

            ReloadProject();
            monitor.Watch();
        }

        private void Monitor_WatchNotifyChange(object sender, FSMonitorEventArgs e)
        {
            var efn = e.Info.Filename.ToLower();
            if (efn.EndsWith(".csproj"))
            {
                ReloadProject();
            }
            else if (efn.EndsWith(".cs"))
            {
                RootFolder.ProcessChangeEvent(e.Info);
            }
        }

        public ElementType ElementType => ElementType.Project;

        public override string ToString()
        {
            return Title;
        }

        public void Dispose()
        {
            ((IDisposable)monitor).Dispose();
            monitor = null; 
        }

        ~CurioProject()
        {
            monitor?.Dispose();
        }

    }


}


using DataTools.CSTools;
using DataTools.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DataTools.CSTools
{

    public enum ElementType
    {
        Project,
        Directory,
        File,
        Marker
    }


    public interface IProjectElement
    {
        string Title { get; }

        ElementType ElementType { get; }

    }

    public interface IProjectHost : IProjectElement
    {
        IProjectElement Project { get; }
    }

    public interface IProjectNode : IProjectElement
    {
        ObservableCollection<IProjectElement> Children { get; }
    }


    public class CurioProject : ObservableBase, IProjectHost
    {
        private string projectRoot = "";
        private string projectFile = "";
        private EnvDTE.Project _project;

        private CSDirectory projectFolder = null;

        private object selectedItem = null;

        private XmlDocument xml;
        private string title = null;

        private List<string> allNamespaces = new List<string>();
        private string defns;
        private string assyname;

        public string DefaultNamespace => defns;

        public string AssemblyName => assyname;

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

        public CSDirectory ProjectFolder
        {
            get => projectFolder;
            protected set
            {
                if (SetProperty(ref projectFolder, value))
                {
                    OnPropertyChanged(nameof(Project));
                }
            }
        }

        public IProjectElement Project => projectFolder;

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

            ProjectFolder = new CSDirectory(this, ProjectRoot);

            allNamespaces.Clear();
            allNamespaces.Add(defns);
            allNamespaces.AddRange(ProjectFolder.GetAllNamespacesFromHere());
            allNamespaces = allNamespaces.Distinct().ToList();
            allNamespaces.Sort();

            OnPropertyChanged(nameof(Namespaces));
        }

        private string PopulateProjectProperties()
        {
            var en = _project.Properties;

            foreach (EnvDTE.Property prop in en)
            {
                if (prop.Name == "DefaultNamespace")
                {

                    if (prop.Value is string s)
                    {
                        defns = s;
                    }
                }
                else if (prop.Name == "AssemblyName")
                {

                    if (prop.Value is string s)
                    {
                        assyname = s;
                    }
                }
            }

            return null;
        }

        public CurioProject(string filename, EnvDTE.Project nativeProject)
        {
            _project = nativeProject;            
            PopulateProjectProperties();

            if (filename == null || !File.Exists(filename)) throw new FileNotFoundException();

            ProjectRoot = Path.GetFullPath(Path.GetDirectoryName(filename) ?? "\\");
            ProjectFile = Path.GetFileName(filename);   

            ReloadProject();
        }

        public ElementType ElementType => ElementType.Project;

        public override string ToString()
        {
            return Title;
        }

    }
}

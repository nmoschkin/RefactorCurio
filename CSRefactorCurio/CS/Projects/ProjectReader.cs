
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


    public class ProjectReader : ObservableBase, IProjectHost
    {
        private string projectRoot = "";
        private string projectFile = "";

        private CSDirectory projectFolder = null;

        private object selectedItem = null;

        private XmlDocument xml;
        private string title = null;

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
            set
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
            set
            {
                SetProperty(ref projectRoot, value);
            }
        }

        public string ProjectFile
        {
            get => projectFile;
            set
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


        private void LoadProject()
        {
            xml = new XmlDocument();
            xml.LoadXml(File.ReadAllText($"{ProjectRoot}\\{ProjectFile}"));

            ProjectFolder = new CSDirectory(this, ProjectRoot);
        }


        public ProjectReader(string filename)
        {
            if (filename == null || !File.Exists(filename)) throw new FileNotFoundException();

            ProjectRoot = Path.GetFullPath(Path.GetDirectoryName(filename) ?? "\\");
            ProjectFile = Path.GetFileName(filename);   

            LoadProject();
        }

        public ElementType ElementType => ElementType.Project;

        public ProjectReader()
        {

        }

        

    }
}

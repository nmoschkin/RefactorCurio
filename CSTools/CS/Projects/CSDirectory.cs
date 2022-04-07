
using DataTools.CSTools;
using DataTools.MathTools;
using DataTools.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DataTools.CSTools
{

    public class CSMarker : Marker, IProjectElement
    {
        public ElementType ElementType => ElementType.Marker;

        public string Title
        {
            get => this.Name;
        }
    }

    public class CSCodeFile : CSCodeParser, IProjectNode, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string title;

        private ObservableCollection<IProjectElement> children = new ObservableCollection<IProjectElement>();
        public ElementType ElementType => ElementType.File;

        public ObservableCollection<IProjectElement> Children
        {
            get => children;
            private set => children = value; 
        }

        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                }
            }
        }

        public override string Filename 
        { 
            get => base.Filename; 
            protected set
            {
                if (base.Filename != value)
                {
                    base.Filename = value;
                    if (value != null)
                    {
                        Title = System.IO.Path.GetFileNameWithoutExtension(value);
                    }

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Filename)));
                }
            }
        }

        new public static CSCodeFile LoadFromFile(string path)
        {
            var cf = new CSCodeFile();
            cf.LoadFile(path);
            return cf;
        }

        protected override bool Parse(string text)
        {
            children.Clear();
            if (base.Parse(text) && markers != null)
            {
                foreach(var marker in markers)
                {
                    var mnew = new CSMarker();
                    ObjectMerge.MergeObjects(marker, mnew);

                    children.Add(mnew);
                }

                return true;
            }

            return false;
        }
    }

    public class CSDirectory : ObservableBase, IProjectNode
    {

        private WeakReference<CSDirectory> parent = null;
        private WeakReference<ProjectReader> project = null;

        private ObservableCollection<CSCodeFile> files;
        private ObservableCollection<CSDirectory> directories;

        private ObservableCollection<IProjectElement> children;

        private string path = null;
        private string title = null;

        public ElementType ElementType => ElementType.Directory;

        public string Title
        {
            get => title;
            protected set
            {
                SetProperty(ref title, value);
            }
        }

        public ObservableCollection<IProjectElement> Children
        {
            get => children;
            protected set
            {
                SetProperty(ref children, value);
            }
        }


        public ProjectReader Project
        {
            get
            {
                if (project != null)
                {
                    if (project.TryGetTarget(out var projectReader))
                    {
                        return projectReader;
                    }
                }

                return null;
            }
        }

        public ObservableCollection<CSDirectory> Directories
        {
            get => directories;
            protected set
            {
                SetProperty(ref directories, value);
            }
        }
        
        public ObservableCollection<CSCodeFile> Files
        {
            get => files;
            protected set
            {
                SetProperty(ref files, value);
            }
        }


        public string Path
        {
            get => path;
            protected set
            {
                if (SetProperty(ref path, value))
                {
                    Title = System.IO.Path.GetFileNameWithoutExtension(value);
                }
            }
        }

        public CSDirectory Parent
        {
            get
            {
                if (parent == null) return null;    

                if (parent.TryGetTarget(out var root))
                {
                    return root;
                }
                return null;
            }
        }

        public CSDirectory(ProjectReader project, string path, CSDirectory parent = null)
        {
            files = new ObservableCollection<CSCodeFile>();
            directories = new ObservableCollection<CSDirectory>();
            children = new ObservableCollection<IProjectElement>();

            if (parent != null)
            {
                this.parent = new WeakReference<CSDirectory>(parent);
            }
            
            this.project = new WeakReference<ProjectReader>(project);
            Path = path;

            ReadDirectory();
        }

        public void ReadDirectory(string path = null)
        {
            var p = Project;
            if (p == null) return;  

            path = path ?? this.path;

            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

            Path = path;

            var outfiles = new List<CSCodeFile>();
            var outdirs = new List<CSDirectory>();

            var files = Directory.GetFiles(path, "*.cs");

            foreach (var file in files)
            {

                try
                {
                    var parser = CSCodeFile.LoadFromFile(file);
                    outfiles.Add(parser);
                }
                catch { }
            }

            Files = new ObservableCollection<CSCodeFile>(outfiles);

            var dirs = Directory.GetDirectories(path);

            foreach (var dir in dirs)
            {
                switch (System.IO.Path.GetFileName(dir))
                {
                    case ".vs":
                    case ".vscode":
                    case "bin":
                    case "obj":
                    case ".git":
                        continue;

                    default:
                        break;
                }

                outdirs.Add(new CSDirectory(p, dir, this));
            }

            Directories = new ObservableCollection<CSDirectory>(outdirs);

            Children.Clear();

            foreach (var item1 in outfiles)
            {
                Children.Add(item1);
            }

            foreach (var item2 in outdirs)
            {
                Children.Add(item2);
            }

        }

    }
}

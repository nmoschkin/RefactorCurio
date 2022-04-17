
using DataTools.CSTools;
using DataTools.Desktop;
using DataTools.MathTools;
using DataTools.Observable;
using DataTools.SortedLists;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataTools.CSTools
{

    public class CSMarker : MarkerBase<CSMarker, ObservableMarkerList<CSMarker>>, IProjectNode<ObservableMarkerList<CSMarker>>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public override ObservableMarkerList<CSMarker> Children 
        { 
            get => base.Children;
            set
            {
                if (base.Children != value)
                {
                    base.Children = value;
                    OnPropertyChanged();
                }
            }
        
        }


        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CSCodeFile : CSCodeParser<CSMarker, ObservableMarkerList<CSMarker>>, IProjectNode<ObservableMarkerList<CSMarker>>, INotifyPropertyChanged, IMarkerFilterProvider<CSMarker, ObservableMarkerList<CSMarker>>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableMarkerList<CSMarker> filteredChildren;

        private string title;
        
        CSFileChain<CSMarker, ObservableMarkerList<CSMarker>> fileChain = new CSFileChain<CSMarker, ObservableMarkerList<CSMarker>>();
 
        public ElementType ElementType => ElementType.File;
        public ElementType ChildType => ElementType.Marker;

        IList IProjectNode.Children => markers;

        bool nocolnotify = false;

        public MarkerFilter<CSMarker, ObservableMarkerList<CSMarker>> Filter { get; } = new MarkerFilter<CSMarker, ObservableMarkerList<CSMarker>>();

        public virtual ObservableMarkerList<CSMarker> FilteredItems
        {
            get => filteredChildren;
            protected set
            {
                if (filteredChildren != value)
                {
                    filteredChildren = value;
                    OnPropertyChanged();
                }
                
            }
        }

        public virtual ObservableMarkerList<CSMarker> Children
        {
            get => markers;
            protected set
            {
                if (markers != value)
                {
                    if (markers != null)
                    {
                        markers.CollectionChanged -= OnChildrenChanged;
                    }
                    markers = value;
                    if (markers != null)
                    {
                        markers.CollectionChanged += OnChildrenChanged;
                    }

                    RunFilters(markers);

                    OnPropertyChanged();
                }
            }
        }

        public MarkerFilterRule ProvideFilterRule(ObservableMarkerList<CSMarker> items)
        {
            return fileChain; 
        }

        public ObservableMarkerList<CSMarker> RunFilters(ObservableMarkerList<CSMarker> items)
        {
            FilteredItems = Filter.ApplyFilter(items, ProvideFilterRule(items));
            return FilteredItems;
        }

        public string Rename(string newName)
        {
            var oldname = Filename;

            if (File.Exists(Filename))
            {
                try
                {
                    File.Move(Filename, newName);
                    Filename = System.IO.Path.GetFullPath(newName);
                }
                catch
                {
                    return null;
                }
            }

            return oldname;
        }

        internal void RenameEvent(string newName)
        {
            Filename = newName;
        }

        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged();
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
                        Title = System.IO.Path.GetFileName(value);
                    }

                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        new public static CSCodeFile LoadFromFile(string path)
        {
            var cf = new CSCodeFile();
            cf.LoadFile(path);
            return cf;
        }
        
        public void AddMarker(CSMarker marker)
        {
            if (!markers.Contains(marker)) markers.Add(marker);
        }

        public bool RemoveMarker(CSMarker marker)
        {
            return markers.Remove(marker);
        }

        protected override bool Parse(string text)
        {
            markers.Clear();
            nocolnotify = true;

            if (base.Parse(text) && markers != null)
            {
                RunFilters(markers);
                nocolnotify = false;
                return true;
            }

            nocolnotify = false;
            return false;
        }

        public CSCodeFile()
        {
            markers.CollectionChanged += OnChildrenChanged;
        }

        private void OnChildrenChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!nocolnotify) RunFilters(markers);
        }
    }

    public class CSDirectory : ObservableBase, IProjectNode<ObservableCollection<IProjectElement>>
    {

        private WeakReference<CSDirectory> parent = null;
        private WeakReference<CurioProject> project = null;

        private ObservableCollection<CSCodeFile> files;
        private ObservableCollection<CSDirectory> directories;

        private ObservableCollection<IProjectElement> children;

        private string path = null;
        private string title = null;

        private List<string> namespaces;

        public ElementType ElementType => ElementType.Directory;
        public ElementType ChildType => ElementType.File;

        IList IProjectNode.Children => children;

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


        public CurioProject Project
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
                if (SetProperty(ref files, value))
                {
                    CheckNamespaces();
                }
            }
        }

        public IReadOnlyList<string> Namespaces
        {
            get => Namespaces;
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

        public CSDirectory(CurioProject project, string path, CSDirectory parent = null)
        {
            files = new ObservableCollection<CSCodeFile>();
            directories = new ObservableCollection<CSDirectory>();
            children = new ObservableCollection<IProjectElement>();

            if (parent != null)
            {
                this.parent = new WeakReference<CSDirectory>(parent);
            }
            
            this.project = new WeakReference<CurioProject>(project);
            Path = path;

            ReadDirectory();
        }

        public List<string> GetAllNamespacesFromHere()
        {
            CheckNamespaces();

            List<string> ns = new List<string>();
            ns.AddRange(namespaces);

            foreach (var dir in directories)
            {
                ns.AddRange(dir.GetAllNamespacesFromHere());
            }

            return ns.Distinct().ToList();
        }

        public bool RemovePath(string path)
        {
            var obj = Find(path);
            if (obj is IProjectElement e)
            {
                return Remove(e);
            }

            return false;
        }

        public bool Remove(IProjectElement item)
        {
            if (item is IProjectNode node)
            {
                if (children.Contains(node))
                {
                    children.Remove(node);
                    if (node is CSDirectory dir)
                    {
                        directories.Remove(dir);
                    }                    
                    else if (node is CSCodeFile file)
                    {
                        files.Remove(file);
                    }

                    return true;
                }

                foreach (var dir in directories)
                {
                    if (dir.Remove(item)) return true;
                }
            }
            else if (item is CSMarker child)
            {
                foreach (IProjectNode pnode in children)
                {
                    if (pnode.Children.Contains(item))
                    {
                        if (pnode is CSCodeFile file)
                        {
                            file.RemoveMarker(child);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void ProcessChangeEvent(FileNotifyInfo info)
        {
            var s = this.Project.ProjectRootPath + "\\" + info.Filename;

            switch (info.Action)
            {
                case FileActions.Added:
                    Find(s, true);
                    break;

                case FileActions.Removed:
                    RemovePath(s);
                    break;

                case FileActions.RenamedOldName:
                    var obj = Find(this.Project.ProjectRootPath + "\\" + info.OldName);
                    if (obj != null)
                    {

                        if (obj is CSCodeFile file)
                        {
                            if (File.Exists(file.Filename)) file.Refresh();

                            if (!info.NewName.EndsWith("TMP"))
                            {
                                file.RenameEvent(this.Project.ProjectRootPath + "\\" + info.NewName);
                            }
                        }
                        else if (obj is CSDirectory dir)
                        {
                            dir.path = this.Project.ProjectRootPath + "\\" + info.NewName;
                        }
                    }
                    break;

                case FileActions.Modified:
                    var fobj = FindFile(s);
                    if (fobj != null)
                    {
                        fobj.Refresh();
                    }
                    break;

            }
        }

        public IProjectElement Find(string path, bool create = false)
        {
            if (Directory.Exists(path))
            {
                return FindDirectory(path, create);
            }
            else if (File.Exists(path))
            {
                return FindFile(path, create);
            }
            else
            {
                return null;
            }
        }

        public CSDirectory FindDirectory(string path, bool create = false)
        {
            var fp = System.IO.Path.GetFullPath(path).ToLower();
            var mp = this.path.ToLower();

            if (mp == fp) return this;

            if (fp.StartsWith(mp))
            {
                foreach (var item in Directories)
                {
                    var result = item.FindDirectory(path);
                    if (result != null) return result;
                }

                if (create)
                {
                    var tp = path.Substring(mp.Length);
                    if (!tp.Contains(System.IO.Path.DirectorySeparatorChar))
                    {
                        var newdir = new CSDirectory(Project, path, this);
                        
                        directories.Add(newdir);
                        children.Add(newdir);

                        Sort();

                        return newdir;
                    }
                }

            }

            return null;
        }

        public CSCodeFile FindFile(string filename, bool create = false)
        {
            var fp = System.IO.Path.GetFullPath(filename).ToLower();

            var pathpart = System.IO.Path.GetDirectoryName(fp);
            var fnpart = System.IO.Path.GetFileName(fp);

            if (this.path.ToLower() == pathpart)
            {
                foreach (var file in Files)
                {
                    var fnpart2 = System.IO.Path.GetFileName(file.Filename).ToLower();
                    if (fnpart2 == fnpart) return file;
                }

                if (create)
                {
                    var csnew = CSCodeFile.LoadFromFile(filename);                    
                    files.Add(csnew);
                    children.Add(csnew);

                    Sort();
                    return csnew;
                }
            }

            foreach (var dir in Directories)
            {
                var cf = dir.FindFile(filename);
                if (cf != null) return cf;  
            }

            return null;
        }
        
        public void ReadDirectory(string path = null)
        {
            var p = Project;
            var isf = p.IsFrameworkProject;
            var rt = p.ProjectRootPath;

            if (p == null) return;  

            path = path ?? this.path;

            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

            Path = path;

            var outfiles = new List<CSCodeFile>();
            var outdirs = new List<CSDirectory>();

            var files = Directory.GetFiles(path, "*.cs");
            string file;

            foreach (var f in files)
            {
                file = f;

                if (isf)
                {
                    file = file.Replace(rt + "\\", "");
                    if (!p.Includes.Contains(file)) continue;
                }
                else
                {
                    file = file.Replace(rt + "\\", "");
                    if (p.Excludes.Contains(file)) continue;
                }

                try
                {
                    var parser = CSCodeFile.LoadFromFile(f);
                    outfiles.Add(parser);
                }
                catch (Exception ex)
                {
                    var b = new MessageBox();
                    b.ShowError(ex.Message, ex.StackTrace);
                }
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

            foreach (var item2 in outdirs)
            {
                Children.Add(item2);
            }

            foreach (var item1 in outfiles)
            {
                Children.Add(item1);
            }

        }

        public void Sort(bool descending = false)
        {
            var m = descending ? -1 : 1;

            QuickSort.Sort(children, (a, b) =>
            {
                if (a.ElementType == ElementType.Directory && b.ElementType != ElementType.Directory) return -1 * m;
                else if (a.ElementType != ElementType.Directory && b.ElementType == ElementType.Directory) return 1 * m;
                else return string.Compare(a.Title, b.Title) * m;
            });

            QuickSort.Sort(directories, (a, b) =>
            {
                return string.Compare(a.Title, b.Title);
            });

            QuickSort.Sort(files, (a, b) =>
            {
                return string.Compare(a.Title, b.Title);
            });

        }

        protected void CheckNamespaces()
        {

            List<string> p = new List<string>();

            foreach (var item in Files)
            {
                foreach(var marker in item.Markers)
                {
                    if (!p.Contains(marker.Namespace))
                    {
                        p.Add(marker.Namespace);
                    }
                }
            }

            namespaces = p;
            OnPropertyChanged(nameof(Namespaces));
        }

        public override string ToString()
        {
            return Title;
        }

    }
}

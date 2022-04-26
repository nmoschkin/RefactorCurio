
using DataTools.CSTools;
using DataTools.Desktop;
using DataTools.MathTools;
using DataTools.Observable;
using DataTools.SortedLists;

using Microsoft.Build.Framework.XamlTypes;

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
    /// <summary>
    /// CS Refactor Curio Solution Source Code File based on <see cref="CSCodeParser{TMarker, TList}"/>.
    /// </summary>
    public class CSCodeFile : CSCodeParser<CSMarker, ObservableMarkerList<CSMarker>>, IProjectFile<ObservableMarkerList<CSMarker>>, INotifyPropertyChanged, IMarkerFilterProvider<CSMarker, ObservableMarkerList<CSMarker>>
    {
        #region Private Fields

        CSProjectDisplayChain<CSMarker, ObservableMarkerList<CSMarker>> fileChain = new CSProjectDisplayChain<CSMarker, ObservableMarkerList<CSMarker>>();

        private ObservableMarkerList<CSMarker> filteredChildren;

        bool nocolnotify = false;

        private string title;

        private WeakReference<CurioProject> project;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Instantiate a blank, unloaded code file reader.
        /// </summary>
        public CSCodeFile(CurioProject project)
        {
            Project = project;
            markers.CollectionChanged += OnChildrenChanged;
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        IList IProjectNode.Children => markers;
        public virtual ObservableMarkerList<CSMarker> Children
        {
            get
            {
                if ((markers == null || markers.Count == 0) && IsLazyLoad) Refresh();
                return markers;
            }
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

        public ElementType ChildType => ElementType.Marker;
        public ElementType ElementType => ElementType.File;
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

        public MarkerFilter<CSMarker, ObservableMarkerList<CSMarker>> Filter { get; } = new MarkerFilter<CSMarker, ObservableMarkerList<CSMarker>>();

        public virtual ObservableMarkerList<CSMarker> FilteredItems
        {
            get
            {
                if ((markers == null || markers.Count == 0) && IsLazyLoad) Refresh();
                return filteredChildren;
            }
            protected set
            {
                if (filteredChildren != value)
                {
                    filteredChildren = value;
                    OnPropertyChanged();
                }

            }
        }

        public CurioProject Project
        {
            get
            {
                if (project == null || !project.TryGetTarget(out var proj))
                {
                    return null;
                }
                return proj;
            }
            protected set
            {
                if (value == null)
                {
                    project = null;
                }
                else
                {
                    project = new WeakReference<CurioProject>(value);
                }
            }
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

        #endregion Public Properties

        #region Public Methods

        new public static CSCodeFile LoadFromFile(string path, CurioProject project, bool lazy)
        {
            var cf = new CSCodeFile(project);
            cf.LoadFile(path, lazy);
            return cf;
        }

        /// <summary>
        /// Add the specified marker to the children collection if not there already.
        /// </summary>
        /// <param name="marker">The marker to add.</param>
        public void AddMarker(CSMarker marker)
        {
            if (!markers.Contains(marker)) markers.Add(marker);
        }

        public MarkerFilterRule ProvideFilterRule(ObservableMarkerList<CSMarker> items)
        {
            return fileChain;
        }

        /// <summary>
        /// Remove the specified marker.
        /// </summary>
        /// <param name="marker"></param>
        /// <returns>True if the marker was located and removed successfully.</returns>
        public bool RemoveMarker(CSMarker marker)
        {
            return markers.Remove(marker);
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

        public ObservableMarkerList<CSMarker> RunFilters(ObservableMarkerList<CSMarker> items)
        {
            FilteredItems = Filter.ApplyFilter(items, ProvideFilterRule(items));
            return FilteredItems;
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Fired when the file is renamed from outside the solution.
        /// </summary>
        /// <param name="newName"></param>
        internal void RenameEvent(string newName)
        {
            Filename = newName;
        }

        #endregion Internal Methods

        #region Protected Methods

        public override ObservableMarkerList<CSMarker> GetMarkersForCommit()
        {
            var db = new CSXMLIntegratorFilter<CSMarker, ObservableMarkerList<CSMarker>>();
            return db.ApplyFilter(markers);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected override bool Parse(string text)
        {
            markers.Clear();
            nocolnotify = true;

            if (base.Parse(text) && markers != null)
            {
                SetHomeFile(markers);
                RunFilters(markers);

                nocolnotify = false;
                return true;
            }

            nocolnotify = false;
            return false;
        }


        #endregion Protected Methods

        #region Private Methods

        private void OnChildrenChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!nocolnotify) RunFilters(markers);
        }

        private void SetHomeFile(IMarkerList markers)
        {
            foreach (IMarker item in markers)
            {
                item.HomeFile = this;
                SetHomeFile(item.Children);
            }
        }

        #endregion Private Methods
    }

    /// <summary>
    /// CS Refactor Curio Solution Source Code Directory object.
    /// </summary>
    public class CSDirectory : ObservableBase, IProjectNode<ObservableCollection<IProjectElement>>
    {

        #region Private Fields

        private ObservableCollection<IProjectElement> children;
        private ObservableCollection<CSDirectory> directories;
        private ObservableCollection<CSCodeFile> files;
        private List<string> namespaces;
        private WeakReference<CSDirectory> parent = null;
        private string path = null;
        private WeakReference<CurioProject> project = null;
        private string title = null;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Create a new directory element for the specified project.
        /// </summary>
        /// <param name="project">The project to create the element for.</param>
        /// <param name="path">The valid path to the folder.</param>
        /// <param name="parent">Optional parent subfolder within the specified project.</param>
        public CSDirectory(CurioProject project, string path, CSDirectory parent = null)
        {
            if (parent != null && project != parent.Project) throw new ArgumentException("Parent directory must be a member of the same project being referenced.");

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

        #endregion Public Constructors

        #region Public Properties

        IList IProjectNode.Children => children;
        public ObservableCollection<IProjectElement> Children
        {
            get => children;
            protected set
            {
                SetProperty(ref children, value);
            }
        }

        public ElementType ChildType => ElementType.File;
        /// <summary>
        /// Gets the observable list of subdirectories.
        /// </summary>
        public ObservableCollection<CSDirectory> Directories
        {
            get => directories;
            protected set
            {
                SetProperty(ref directories, value);
            }
        }

        public ElementType ElementType => ElementType.Directory;
        /// <summary>
        /// Gets the observable list of source code files.
        /// </summary>
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

        /// <summary>
        /// Gets the current list of namespaces (or empty if not loaded)
        /// </summary>
        public IReadOnlyList<string> Namespaces
        {
            get => Namespaces;
        }

        /// <summary>
        /// Gets the parent directory or null if this is the project root folder.
        /// </summary>
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

        /// <summary>
        /// Gets the absolute path of the current directory.
        /// </summary>
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

        /// <summary>
        /// Gets the parent project.
        /// </summary>
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

        public string Title
        {
            get => title;
            protected set
            {
                SetProperty(ref title, value);
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Find the project element at the location specified by <paramref name="path"/>, optionally creating it, if it does not exist.
        /// </summary>
        /// <param name="path">The path for the project element to retrieve.</param>
        /// <param name="create">True to create the element if it does not exist.</param>
        /// <returns>An element or null.</returns>
        /// <remarks>
        /// The directory must be a subdirectory of the one being referenced by this object or nothing will be returned, even if the object exists on disk.
        /// </remarks>
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

        /// <summary>
        /// Find the project directory at the location specified by <paramref name="path"/>, optionally creating it, if it does not exist.
        /// </summary>
        /// <param name="path">The path for the project directory to retrieve.</param>
        /// <param name="create"></param>
        /// <returns>A directory object or null.</returns>
        /// <remarks>
        /// The path must already exist on disk.
        /// </remarks>
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

        /// <summary>
        /// Find the project file at the location specified by <paramref name="filename"/>, optionally creating it, if it does not exist.
        /// </summary>
        /// <param name="filename">The path for filename of the project file to retrieve.</param>
        /// <param name="create"></param>
        /// <returns>A directory object or null.</returns>
        /// <remarks>
        /// The file must already exist on disk.
        /// </remarks>
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
                    var csnew = CSCodeFile.LoadFromFile(filename, Project, true);

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

        /// <summary>
        /// Gets all namespaces that can be found in all files in all subfolders of this directory.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the total number of recognized files under this directory (including in all subdirectories)
        /// </summary>
        /// <returns></returns>
        public int GetTotalFilesCount()
        {
            var count = files.Count;

            foreach (var dir in directories)
            {
                count += dir.GetTotalFilesCount();
            }

            return count;
        }
        /// <summary>
        /// Process a <see cref="FileNotifyInfo"/> change event from the filesystem watcher.
        /// </summary>
        /// <param name="info">The info to parse.</param>
        /// <remarks>
        /// If the project file, itself, changes, the entire subdirectory structure is updated.
        /// Otherwise, only the affected file is updated.
        /// </remarks>
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
                    var parser = CSCodeFile.LoadFromFile(f, Project, true);
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

        /// <summary>
        /// Remove the specified project element from the project (does not delete files)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Remove the object at the specified path from the project (does not delete the file on disk)
        /// </summary>
        /// <param name="path">The file or directory to remove.</param>
        /// <returns>True if successful.</returns>
        public bool RemovePath(string path)
        {
            var obj = Find(path);
            if (obj is IProjectElement e)
            {
                return Remove(e);
            }

            return false;
        }
        /// <summary>
        /// Sort child elements.
        /// </summary>
        /// <param name="descending">True to sort in reverse.</param>
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

        public override string ToString()
        {
            return Title;
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Performs a check to ensure the local list of namespaces is up to date with the structure of the directory.
        /// </summary>
        protected void CheckNamespaces()
        {

            List<string> p = new List<string>();

            foreach (var item in Files)
            {
                foreach (var marker in item.Markers)
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

        #endregion Protected Methods
    }

    /// <summary>
    /// CS Refactor Curio Solution Marker
    /// </summary>
    public class CSMarker : MarkerBase<CSMarker, ObservableMarkerList<CSMarker>>, IProjectNode<ObservableMarkerList<CSMarker>>
    {
        #region Public Methods

        public override CSMarker FindParent(MarkerKind parentKind)
        {
            var p = this.ParentElement as CSMarker;

            while (p != null)
            {
                if (p.Kind == parentKind) return p;
                p = p.ParentElement as CSMarker;
            }

            return null;
        }

        #endregion Public Methods


#if !EXPERIMENTAL
        private void Shenanigans1(string text)
        {
            var str1 = "This is some shenanigans";
#else
        private void Shenanigans2(string text)
        {
            var str2 = "This is some other shenanigans";
#endif

        }

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        new public CSCodeFile HomeFile
        {
            get => (CSCodeFile)base.HomeFile;
            set => base.HomeFile = value;
        }

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

        public string[] ExtractAllUsings()
        {

            List<string> usings = new List<string>();

            if (kind == MarkerKind.Using)
            {
                usings.Add(Name);
            }

            foreach (var c in Children)
            {
                usings.AddRange(c.ExtractAllUsings());
            }

            return usings.ToArray();
        }


        public string[] ExtractAllGlobalUsings()
        {

            List<string> usings = new List<string>();

            if (kind == MarkerKind.Using && AccessModifiers == AccessModifiers.Global)
            {
                usings.Add(Name);
            }

            foreach (var c in Children)
            {
                usings.AddRange(c.ExtractAllUsings());
            }

            return usings.ToArray();
        }

        #endregion Public Properties

        #region Protected Methods

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Protected Methods

    }
}
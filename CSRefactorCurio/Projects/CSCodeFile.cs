using DataTools.Code.CS;
using DataTools.Code.Filtering;
using DataTools.Code.Markers;
using DataTools.Code.Project;

using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace DataTools.CSTools
{
    /// <summary>
    /// CS Refactor Curio Solution Source Code File based on <see cref="CSCodeParser{TMarker, TList}"/>.
    /// </summary>
    internal class CSCodeFile : CSCodeParser<CSMarker, ObservableMarkerList<CSMarker>>, IProjectFile<ObservableMarkerList<CSMarker>>, INotifyPropertyChanged, IMarkerFilterProvider<CSMarker, ObservableMarkerList<CSMarker>>
    {
        private CSProjectDisplayChain<CSMarker, ObservableMarkerList<CSMarker>> fileChain = new CSProjectDisplayChain<CSMarker, ObservableMarkerList<CSMarker>>();

        private ObservableMarkerList<CSMarker> filteredChildren;

        private bool nocolnotify = false;

        private string title;

        private WeakReference<CurioProject> project;

        /// <summary>
        /// Instantiate a blank, unloaded code file reader.
        /// </summary>
        public CSCodeFile(CurioProject project)
        {
            Project = project;
            markers.CollectionChanged += OnChildrenChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        IEnumerable IProjectNode.Children => markers;

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

        public new static CSCodeFile LoadFromFile(string path, CurioProject project, bool lazy)
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

        /// <summary>
        /// Fired when the file is renamed from outside the solution.
        /// </summary>
        /// <param name="newName"></param>
        internal void RenameEvent(string newName)
        {
            Filename = newName;
        }

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
    }
}
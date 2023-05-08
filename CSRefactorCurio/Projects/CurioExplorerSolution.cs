using CSRefactorCurio.Dialogs;
using CSRefactorCurio.Reporting;
using CSRefactorCurio.Services;
using CSRefactorCurio.ViewModels;

using DataTools.Code.CS.Filtering;
using DataTools.Code.Project;
using DataTools.CSTools;

using EnvDTE80;
using Microsoft.Internal.VisualStudio.PlatformUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CSRefactorCurio.Projects
{
    /// <summary>
    /// The heart of the Curio Refactor Studio Solution
    /// </summary>
    internal class CurioExplorerSolution : ViewModelBase, ISolution
    {
        private EnvDTE.Solution _sln;
        private int classMode = 0;
        private List<ObservableCollection<IProjectElement>> classModes;
        private IOwnedCommand clickBuild;
        private IOwnedCommand clickClasses;
        private IOwnedCommand clickNamespace;
        private IOwnedCommand clickProject;
        private IOwnedCommand clickFilter;
        private IOwnedCommand clickViewModel;
        private Cursor cursor = Cursors.Arrow;
        
        private Dictionary<string, CSNamespace> namespacesMap = new Dictionary<string, CSNamespace>();
        private IOwnedCommand reportCommand;
        private object selectedItem;
        private IOwnedCommand splitFileCommand;
        private CodeFilterOptions filterOptions = new CodeFilterOptions();

        /// <summary>
        /// Create a new Curio Solution
        /// </summary>
        public CurioExplorerSolution() : base(false, false, false, false)
        {
            classModes = new List<ObservableCollection<IProjectElement>>()
            {
                new ObservableCollection<IProjectElement>(),
                new ObservableCollection<IProjectElement>(),
                new ObservableCollection<IProjectElement>()
            };

            foreach (var col in classModes)
            {
                col.CollectionChanged += Projects_CollectionChanged;
            }

            clickNamespace = new OwnedCommand(this, (o) =>
            {
                //ClassMode = 1;
            }, nameof(ClickNamespace));

            clickClasses = new OwnedCommand(this, (o) =>
            {
                //ClassMode = 0;
            }, nameof(ClickClasses));

            clickBuild = new OwnedCommand(this, (o) =>
            {
                _sln?.SolutionBuild?.Build();
            }, nameof(ClickBuild));

            reportCommand = new OwnedCommand(this, (o) =>
            {
                var dlg = new Report(this);
                dlg.Show();
            }, nameof(ReportCommand));

            splitFileCommand = new OwnedCommand(this, (o) =>
            {
                if (SelectedItem is CSCodeFile cf)
                {
                    var p = cf.Project.RootFolder.Find(Path.GetDirectoryName(cf.Filename));

                    cf.OutputMarkers(Path.GetDirectoryName(cf.Filename));
                }
            }, nameof(SplitFileCommand));

            clickProject = new OwnedCommand(this, (o) =>
            {
                if (SelectedItem is CurioProject proj && proj.Properties is PropertiesContainer c)
                {
                    var l = new List<string>();

                    foreach (var p in c)
                    {
                        l.Add(p.Name);
                    }
                }
            }, nameof(ClickProject));

            clickFilter = new OwnedCommand(this, (o) =>
            {
                var dlg = new FilterDialog(filterOptions);
                dlg.ShowDialog();
            }, nameof(ClickFilter));

            clickViewModel = new OwnedCommand(this, (o) =>
            {
                if (SelectedItem is CSMarker cs && cs.Kind == DataTools.Code.Markers.MarkerKind.Class)
                {
                    var dlg = new EmitDialog(cs);
                    dlg.ShowDialog();
                }
            });

            AutoRegisterCommands(this);
        }

        /// <summary>
        /// Create a new instance from the specified native solution.
        /// </summary>
        /// <param name="sln"></param>
        public CurioExplorerSolution(EnvDTE.Solution sln) : this()
        {
            _sln = sln;
        }

        /// <summary>
        /// Gets or sets the display (class) mode.
        /// </summary>
        public int ClassMode
        {
            get => classMode;
            set
            {
                if (SetProperty(ref classMode, value))
                {
                    if (classMode != 0)
                    {
                        _ = RefreshNamespacesAsync();
                    }
                    else
                    {
                        OnPropertyChanged(nameof(CurrentItems));
                    }
                }
            }
        }

        /// <summary>
        /// Click build action
        /// </summary>
        public IOwnedCommand ClickBuild => clickBuild;

        /// <summary>
        /// Click a class
        /// </summary>
        public IOwnedCommand ClickClasses => clickClasses;

        /// <summary>
        /// Click a namespace
        /// </summary>
        public IOwnedCommand ClickNamespace => clickNamespace;

        public IOwnedCommand ClickProject => clickProject;

        public IOwnedCommand ClickFilter => clickFilter;

        public IOwnedCommand ClickViewModel => clickViewModel;

        /// <summary>
        /// Gets the current view items.
        /// </summary>
        public ObservableCollection<IProjectElement> CurrentItems => classModes[classMode];

        /// <summary>
        /// Gets or sets the current cursor over the Curio tool window.
        /// </summary>
        public Cursor Cursor
        {
            get => cursor;
            set
            {
                SetProperty(ref cursor, value);
            }
        }


        /// <summary>
        /// Gets the most-used objects map.
        /// </summary>
        public ObservableCollection<IProjectElement> MostUsedMap
        {
            get => classModes[1];
        }

        /// <summary>
        /// Gets the current namespaces tree.
        /// </summary>
        public ObservableCollection<IProjectElement> Namespaces
        {
            get => classModes[1];
        }

        IList<IProjectElement> ISolution.Namespaces => classModes[1];

        /// <summary>
        /// Gets the current projects tree.
        /// </summary>
        public ObservableCollection<IProjectElement> Projects
        {
            get => classModes[0];
        }

        IList<IProjectElement> ISolution.Projects => classModes[0];

        /// <summary>
        /// Click show report window
        /// </summary>
        public IOwnedCommand ReportCommand => reportCommand;

        /// <summary>
        /// Click split current file using defaults now.
        /// </summary>

        /// <summary>
        /// Gets or sets the selected treenode item.
        /// </summary>
        public object SelectedItem
        {
            get => selectedItem;
            set
            {
                SetProperty(ref selectedItem, value);
            }
        }

        /// <summary>
        /// Gets the solution.
        /// </summary>
        public EnvDTE.Solution Solution
        {
            get => _sln;
            internal set
            {
                SetProperty(ref _sln, value);
            }
        }

        /// <summary>
        /// Split the current file, now.
        /// </summary>
        public IOwnedCommand SplitFileCommand => splitFileCommand;

        /// <summary>
        /// Gets or sets a value indicating that we are loading the solution and we should not be firing change events until after loading is complete.
        /// </summary>
        protected bool LoadingFlag { get; set; } = false;

        public ElementType ElementType => ElementType.Solution;

        public override string Title => Path.GetFileNameWithoutExtension(Solution?.FullName ?? "");

        /// <summary>
        /// Clear the current solution.
        /// </summary>
        public void Clear()
        {
            LoadingFlag = true;

            foreach (var col in classModes) col.Clear();
            namespacesMap.Clear();

            LoadingFlag = false;
            ClassMode = 0;
        }

        /// <summary>
        /// Find the specified project element by path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IProjectElement FindByPath(string path)
        {
            foreach (IProjectNode project in Projects)
            {
                var s = FindByPath(path, project);
                if (s != null) return s;
            }

            return null;
        }

        /// <summary>
        /// Find the specified <see cref="IProjectElement"/>.
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public EnvDTE.ProjectItem FindProjectItem(IProjectElement elem)
        {
            if (elem is CSMarker marker)
            {
                var hf = marker.HomeFile as CSCodeFile;

                if (hf != null)
                {
                    var np = hf.Project.NativeProject;
                    var fp = hf.Filename.Replace(hf.Project.ProjectRootPath + "\\", "");

                    foreach (EnvDTE.ProjectItem item in np.ProjectItems)
                    {
                        var test = FindProjectItem(fp, item);
                        if (test != null) return test;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find the specified native project item by path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public EnvDTE.ProjectItem FindProjectItem(string path)
        {
            var elem = FindByPath(path);
            if (elem != null)
            {
                return FindProjectItem(elem);
            }

            return null;
        }

        /// <summary>
        /// Load from native solution.
        /// </summary>
        /// <param name="dte"></param>
        public async Task LoadFromDTEAsync(EnvDTE.DTE dte)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Clear();

            LoadingFlag = true;
            Solution = dte.Solution;

            PopulateFrom(Projects, dte.Solution);

            LoadingFlag = false;
            if (classMode != 0) await RefreshNamespacesAsync();
        }

        /// <summary>
        /// Refresh the calculated namespace map.
        /// </summary>
        /// <remarks>
        /// All lazy-load files will be touched and read, at this point.
        /// </remarks>
        public async Task RefreshNamespacesAsync()
        {
            //var project = GetAllProjects(Projects).FirstOrDefault();

            //project.ReadTheFile();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Cursor = Cursors.Wait;
            namespacesMap.Clear();

            var rpt = new NamespaceDistributionReport(this);

            using (var ldr = new NamespaceLoaderService())
            {
                classModes[1] = ldr.NamespacesFromProjects(GetAllProjects(Projects), namespacesMap);
            }

            classModes[2] = rpt.CompilePreparedReport(namespacesMap.Values.ToArray());

            OnPropertyChanged(nameof(Namespaces));
            OnPropertyChanged(nameof(MostUsedMap));

            if (classMode != 0) OnPropertyChanged(nameof(CurrentItems));
            Cursor = null;
        }

        /// <summary>
        /// See if the specified command can be executed.
        /// </summary>
        /// <param name="commandId"></param>
        /// <returns></returns>
        public override bool RequestCanExecute(string commandId)
        {
            if (commandId == nameof(splitFileCommand))
            {
                return SelectedItem is CSCodeFile;
            }

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (Projects != null && Projects.Count > 0)
            {
                DestroyProjects(Projects);
            }
            
            base.Dispose(disposing);
        }

        protected virtual void DestroyProjects(ObservableCollection<IProjectElement> projects)
        {
            foreach (var item in projects)
            {
                if (item is CurioProject proj)
                {
                    proj.Dispose();
                }
                else if (item is CSSolutionFolder sf)
                {
                    DestroyProjects(sf.Children);
                }
            }

            projects.Clear();

        }

        /// <summary>
        /// Populate wrapped items from native items list.
        /// </summary>
        /// <param name="addto">Add new wrapped items to this list.</param>
        /// <param name="source">The source of native items is this list.</param>
        protected void PopulateFrom(IList<IProjectElement> addto, IEnumerable source)
        {
            CSSolutionFolder current = null;
            EnvDTE.Project item;
            foreach (object obj in source)
            {
                if (obj is EnvDTE.Project)
                {
                    item = (EnvDTE.Project)obj;
                }
                else if (obj is EnvDTE.ProjectItem item2 && item2.SubProject is EnvDTE.Project subproj)
                {
                    item = subproj;
                }
                else
                {
                    return;
                }

                if (item.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    current = new CSSolutionFolder(item.Name, this);
                    addto.Add(current);

                    try
                    {
                        PopulateFrom(current.Children, item.ProjectItems);
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);
                    }
                }
                else if (item.FullName.ToLower().EndsWith(".csproj"))
                {
                    try
                    {
                        addto.Add(new CurioProject(item.FullName, item, this));
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);
                    }
                }
            }
        }

        private IProjectElement FindByPath(string path, IProjectNode node)
        {
            if (node is CSSolutionFolder sf)
            {
                foreach (var obj in sf.Children)
                {
                    if (obj is IProjectNode projectNode)
                    {
                        var s = FindByPath(path, projectNode);
                        if (s != null) return s;
                    }
                }
            }
            else if (node is CurioProject proj)
            {
                var s = proj.RootFolder.Find(path);
                if (s != null) return s;
            }

            return null;
        }

        /// <summary>
        /// Find the specified native project item.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="itemSearch"></param>
        /// <param name="currPath"></param>
        /// <returns></returns>
        private EnvDTE.ProjectItem FindProjectItem(string path, EnvDTE.ProjectItem itemSearch, string currPath = null)
        {
            string nn;

            if (!string.IsNullOrEmpty(currPath))
            {
                if (itemSearch.Name.ToLower().EndsWith(".xaml") && itemSearch.ProjectItems.Count > 0)
                {
                    nn = currPath;
                }
                else
                {
                    nn = currPath + "\\" + itemSearch.Name;
                }
            }
            else
            {

                if (itemSearch.Name.ToLower().EndsWith(".xaml") && itemSearch.ProjectItems.Count > 0)
                {
                    nn = "";
                }
                else
                {
                    nn = itemSearch.Name;
                }

            }

            if (nn == path) return itemSearch;

            if (itemSearch.ProjectItems.Count > 0)
            {
                foreach (EnvDTE.ProjectItem item in itemSearch.ProjectItems)
                {
                    if (item == itemSearch) continue;
                    var test = FindProjectItem(path, item, nn);
                    if (test != null) return test;
                }
            }

            return null;
        }

        private List<CurioProject> GetAllProjects(IList<IProjectElement> fromList)
        {
            var output = new List<CurioProject>();

            foreach (var item in fromList)
            {
                if (item is CurioProject proj)
                {
                    output.Add(proj);
                }
                else if (item is CSSolutionFolder sf)
                {
                    output.AddRange(GetAllProjects(sf.Children));
                }
            }

            return output;
        }

        private void Projects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (classMode != 0 && !LoadingFlag) RefreshNamespacesAsync();
        }
    }
}
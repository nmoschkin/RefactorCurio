
using CSRefactorCurio.Dialogs;
using CSRefactorCurio.Reporting;

using DataTools.CSTools;
using DataTools.Observable;

using EnvDTE80;

using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CSRefactorCurio.ViewModels
{


    internal class CurioExplorerSolution : ObservableBase, ICommandOwner, ISolution
    {
        private EnvDTE.Solution _sln;
        private Cursor cursor = Cursors.Arrow;

        private List<ObservableCollection<IProjectElement>> classModes;

        private Dictionary<string, CSNamespace> namespacesMap = new Dictionary<string, CSNamespace>();

        private int classMode = 0;

        private bool[] isActive = new bool[3];

        private IOwnedCommand clickNamespace;
        private IOwnedCommand clickClasses;
        private IOwnedCommand clickBuild;
        private IOwnedCommand reportCommand;
        public IOwnedCommand ClickNamespace => clickNamespace;
        public IOwnedCommand ClickClasses => clickClasses;
        public IOwnedCommand ClickBuild => clickBuild;
        public IOwnedCommand ReportCommand => reportCommand;

        public ObservableCollection<IProjectElement> CurrentItems => classModes[classMode];


        private Dictionary<string, List<INamespace>> allFQN;

        public CurioExplorerSolution()
        {

            classModes = new List<ObservableCollection<IProjectElement>>()
            {
                new ObservableCollection<IProjectElement>(),
                new ObservableCollection<IProjectElement>(),
                new ObservableCollection<IProjectElement>()
            };

            foreach(var col in classModes)
            {
                col.CollectionChanged += Projects_CollectionChanged;
            }

            clickNamespace = new OwnedCommand(this, (o) =>
            {
                ClassMode = 1;
            }, nameof(ClickNamespace));

            clickClasses = new OwnedCommand(this, (o) =>
            {
                ClassMode = 0;
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

            isActive[classMode] = true;
        }

        public bool LoadingFlag { get; set; } = false;

        public void Clear()
        {
            LoadingFlag = true;

            foreach (var col in classModes) col.Clear();
            namespacesMap.Clear();

            classMode = 0;
            LoadingFlag = false;
        }

        private void PopulateFrom(IList<IProjectElement> addto, IEnumerable source)
        {

            DataTools.CSTools.CSSolutionFolder current = null;

            foreach (object obj in source)
            {
                if (obj is EnvDTE.Project item)
                {
                    if (item.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                    {
                        current = new DataTools.CSTools.CSSolutionFolder(item.Name);
                        addto.Add(current);
                        
                        PopulateFrom(current.Children, item.ProjectItems);
                    }
                    else if (item.FullName.ToLower().EndsWith(".csproj"))
                    {
                        addto.Add(new CurioProject(item.FullName, item));
                    }
                }
                else if ((obj is EnvDTE.ProjectItem item2) && (item2.SubProject is EnvDTE.Project subproj))
                {
                    if (subproj.FullName.ToLower().EndsWith(".csproj"))
                    {
                        addto.Add(new CurioProject(subproj.FullName, subproj));
                    }
                }
            }
        }
        public void LoadFromDTE(EnvDTE.DTE dte)
        {
            Clear();
            LoadingFlag = true;

            Solution = dte.Solution;

            PopulateFrom(Projects, dte.Solution);

            LoadingFlag = false;
            if (classMode != 0) RefreshNamespaces();
        }

        private void Projects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (classMode != 0 && !LoadingFlag) RefreshNamespaces();
        }

        private List<CurioProject> GetAllProjects(IList<IProjectElement> fromList)
        {
            var output = new List<CurioProject>();

            foreach(var item in fromList)
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

        public void RefreshNamespaces()
        {

            //var project = GetAllProjects(Projects).FirstOrDefault();

            //project.ReadTheFile();


            Cursor = Cursors.Wait;
            namespacesMap.Clear();

            var rpt = new MostSpreadOutNamespacesReport(this);
            
            
            classModes[1] = CSNamespace.NamespacesFromProjects(GetAllProjects(Projects), namespacesMap, _sln.DTE.StatusBar);
            
            rpt.CompileReport(namespacesMap.Values.ToArray());

            classModes[2] = new ObservableCollection<IProjectElement>(rpt.Reports.Select((x) => (IProjectElement)x));

            OnPropertyChanged(nameof(Namespaces));
            OnPropertyChanged(nameof(MostUsedMap));

            if (classMode == 1) OnPropertyChanged(nameof(CurrentItems));

            Cursor = null;
        }

       
        public CurioExplorerSolution(EnvDTE.Solution sln) : this()
        {
            _sln = sln;
        }

        public Cursor Cursor
        {
            get => cursor;
            set
            {
                SetProperty(ref cursor, value);
            }
        }

        public int ClassMode
        {
            get => classMode;
            set
            {
                if (SetProperty(ref classMode, value))
                {
                    if (classMode != 0)
                    {
                        RefreshNamespaces();
                    }
                    for (int i = 0; i < isActive.Length; i++)
                    {
                        if (i == value) isActive[i] = true;
                        else isActive[i] = false;
                    }

                    OnPropertyChanged(nameof(CurrentItems));

                    OnPropertyChanged(nameof(IsActive1));
                    OnPropertyChanged(nameof(IsActive2));
                    OnPropertyChanged(nameof(IsActive3));
                }
            }
        }

        public EnvDTE.Solution Solution
        {
            get => _sln;
            internal set
            {
                SetProperty(ref _sln, value);   
            }
        }


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

        private EnvDTE.ProjectItem FindProjectItem(string path, EnvDTE.ProjectItem itemSearch, string currPath = null)
        {
            string nn;

            if (currPath != null)
            {
                nn = currPath + "\\" + itemSearch.Name;
            }
            else
            {
                nn = itemSearch.Name;
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

        public EnvDTE.ProjectItem FindProjectItem(string path)
        {
            var elem = FindByPath(path);
            if (elem != null)
            {
                return FindProjectItem(elem);
            }

            return null;
        }

        public IProjectElement FindByPath(string path)
        {

            foreach (IProjectNode project in Projects)
            {
                var s = FindByPath(path, project);
                if (s != null) return s;
            }

            return null;
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


        public ObservableCollection<IProjectElement> Projects
        {
            get => classModes[0];
        }

        public ObservableCollection<IProjectElement> Namespaces
        {
            get => classModes[1];
        }

        public ObservableCollection<IProjectElement> MostUsedMap
        {
            get => classModes[1];
        }

        public bool IsActive1
        {
            get => isActive[0];
            set
            {
                if (!value) return;
                if (SetProperty(ref isActive[0], value) && value && classMode != 0)
                {
                    ClassMode = 0;
                } 
            }
        }

        public bool IsActive2
        {
            get => isActive[1];
            set
            {
                if (!value) return;
                if (SetProperty(ref isActive[1], value) && value && classMode != 1)
                {
                    ClassMode = 1;
                }
            }
        }

        public bool IsActive3
        {
            get => isActive[2];
            set
            {
                if (!value) return;
                if (SetProperty(ref isActive[2], value) && value && classMode != 2)
                {
                    ClassMode = 2;
                }
            }
        }


        IList<IProjectElement> ISolution.Projects => classModes[0];

        IList<IProjectElement> ISolution.Namespaces => classModes[1];

        public bool RequestCanExecute(string commandId)
        {
            return true;
        }
    }
}

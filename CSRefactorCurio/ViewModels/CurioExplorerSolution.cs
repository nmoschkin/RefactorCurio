
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
    internal class CurioExplorerSolution : ObservableBase, ICommandOwner
    {
        private EnvDTE.Solution _sln;
        private Cursor cursor = Cursors.Arrow;

        private ObservableCollection<IProjectElement> projects = new ObservableCollection<IProjectElement>();
        private ObservableCollection<IProjectElement> namespaces = new ObservableCollection<IProjectElement>();

        private Dictionary<string, CSNamespace> namespacesMap = new Dictionary<string, CSNamespace>();

        private bool classMode = true;

        private IOwnedCommand clickNamespace;
        private IOwnedCommand clickClasses;
        private IOwnedCommand clickBuild;
        public IOwnedCommand ClickNamespace => clickNamespace;
        public IOwnedCommand ClickClasses => clickClasses;
        public IOwnedCommand ClickBuild => clickBuild;

        public ObservableCollection<IProjectElement> CurrentItems => classMode ? projects : namespaces;
        private List<string> allFQN;

        public CurioExplorerSolution()
        {
            projects.CollectionChanged += Projects_CollectionChanged;

            clickNamespace = new OwnedCommand(this, (o) =>
            {
                ClassMode = false;
            }, nameof(ClickNamespace));

            clickClasses = new OwnedCommand(this, (o) =>
            {
                ClassMode = true;
            }, nameof(ClickClasses));
            clickBuild = new OwnedCommand(this, (o) =>
            {
                _sln?.SolutionBuild?.Build();
            }, nameof(ClickBuild));
        }

        public bool LoadingFlag { get; set; } = false;

        public void Clear()
        {
            LoadingFlag = true;

            projects.Clear();
            namespaces.Clear();
            namespacesMap.Clear();

            classMode = true;
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
            if (!classMode) RefreshNamespaces();
        }

        private void Projects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!classMode && !LoadingFlag) RefreshNamespaces();
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
            Cursor = Cursors.Wait;
            namespacesMap.Clear();
            namespaces = CSNamespace.NamespacesFromProjects(GetAllProjects(Projects), namespacesMap, _sln.DTE.StatusBar);

            allFQN = AllFullyQualifiedNames(namespacesMap.Values).Distinct().ToList();
            allFQN.Sort();

            OnPropertyChanged(nameof(Namespaces));

            if (!classMode) OnPropertyChanged(nameof(CurrentItems));

            Cursor = null;
        }

        private List<string> AllFullyQualifiedNames(IEnumerable<CSNamespace> namespaces)
        {
            List<string> myfqs = new List<string>();

            foreach (var ns in namespaces)
            {
                myfqs.Add(ns.Name);
                myfqs.AddRange(AllFullyQualifiedNames(ns.Markers));
                myfqs.AddRange(AllFullyQualifiedNames(ns.Namespaces));
            }

            return myfqs;
        } 

        private List<string> AllFullyQualifiedNames(IEnumerable<CSMarker> markers)
        {
            List<string> myfqs = new List<string>();

            foreach (var item in markers)
            {
                myfqs.Add(item.FullyQualifiedName);
                myfqs.AddRange(AllFullyQualifiedNames(item.Children));
            }

            return myfqs;
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

        public bool ClassMode
        {
            get => classMode;
            set
            {
                if (SetProperty(ref classMode, value))
                {
                    if (!classMode)
                    {
                        RefreshNamespaces();
                    }

                    OnPropertyChanged(nameof(CurrentItems));
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


        public ObservableCollection<IProjectElement> Projects
        {
            get => projects;
        }

        public ObservableCollection<IProjectElement> Namespaces
        {
            get => namespaces;
        }

        public bool RequestCanExecute(string commandId)
        {
            return true;
        }
    }
}

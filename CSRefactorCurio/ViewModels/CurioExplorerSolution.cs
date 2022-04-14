
using DataTools.CSTools;
using DataTools.Observable;

using EnvDTE80;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRefactorCurio.ViewModels
{
    internal class CurioExplorerSolution : ObservableBase, ICommandOwner
    {
        private EnvDTE.Solution _sln;

        private ObservableCollection<IProjectElement> projects = new ObservableCollection<IProjectElement>();
        private ObservableCollection<IProjectElement> namespaces = new ObservableCollection<IProjectElement>();

        private Dictionary<string, CSNamespace> namespacesMap = new Dictionary<string, CSNamespace>();

        private bool classMode = true;

        private IOwnedCommand clickNamespace;
        private IOwnedCommand clickClasses;
        public IOwnedCommand ClickNamespace => clickNamespace;
        public IOwnedCommand ClickClasses => clickClasses;

        public ObservableCollection<IProjectElement> CurrentItems => classMode ? projects : namespaces;

        public CurioExplorerSolution()
        {
            projects.CollectionChanged += Projects_CollectionChanged;

            clickNamespace = new OwnedCommand(this, (o) =>
            {
                ClassMode = false;
            }, nameof(ClickNamespace));

            clickClasses = new OwnedCommand(this, (o) =>
            {
                classMode = true;
            }, nameof(ClickClasses));

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
            RefreshNamespaces();
        }

        private void Projects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!LoadingFlag) RefreshNamespaces();
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
            namespacesMap.Clear();
            namespaces = CSNamespace.NamespacesFromProjects(GetAllProjects(Projects), namespacesMap);

            OnPropertyChanged(nameof(Namespaces));
            if (!classMode) OnPropertyChanged(nameof(CurrentItems));
        }

        public CurioExplorerSolution(EnvDTE.Solution sln) : this()
        {
            _sln = sln;
        }

        public bool ClassMode
        {
            get => classMode;
            set
            {
                if (SetProperty(ref classMode, value))
                {
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

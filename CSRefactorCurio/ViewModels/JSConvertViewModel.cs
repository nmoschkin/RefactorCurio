using DataTools.CSTools;
using DataTools.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CSRefactorCurio.ViewModels
{
    internal class JSConvertViewModel : ObservableBase, ICommandOwner
    {

        public event EventHandler<RequestCloseEventArgs> RequestClose;

        private CSJsonClassGenerator generator;
        private CurioProject project;
        private string selNS;

        private ObservableCollection<string> ns = new ObservableCollection<string>();
        private ObservableCollection<CurioProject> projects = new ObservableCollection<CurioProject>();

        private IOwnedCommand okCommand;
        private IOwnedCommand cancelCommand;
        private IOwnedCommand resetCommand;

        public IOwnedCommand OKCommand => okCommand;

        public IOwnedCommand CancelCommand => cancelCommand; 
                
        public IOwnedCommand ResetCommand => resetCommand;


        public ObservableCollection<CurioProject> Projects
        {
            get => projects;
        }

        public CSJsonClassGenerator Generator
        {
            get => generator;
            set
            {
                SetProperty(ref generator, value);
            }
        }

        public CurioProject SelectedProject
        {
            get => project;
            set
            {
                if (value == null)
                {
                    SetProperty(ref project, null);
                    return;
                }
                else if (!projects.Contains(value)) 
                {
                    throw new KeyNotFoundException();
                }
                else
                {
                    if (SetProperty(ref project, value))
                    {
                        ActiveNamespaces = new ObservableCollection<string>(project.Namespaces);
                    }
                }
            }
        }

        public JSConvertViewModel()
        {
            foreach (var p in CSRefectorCurioPackage.CurioSolution.Projects)
            {
                projects.Add(p);
            }

            okCommand = new OwnedCommand(this, (o) =>
            {
                RequestClose?.Invoke(this, new RequestCloseEventArgs(true));
            }, nameof(OKCommand));

            cancelCommand = new OwnedCommand(this, (o) =>
            {
                RequestClose?.Invoke(this, new RequestCloseEventArgs(true));
            }, nameof(CancelCommand));

            resetCommand = new OwnedCommand(this, (o) =>
            {

            }, nameof(ResetCommand));
        }

        public JSConvertViewModel(CurioProject project) : this()
        {
            if (project == null) throw new ArgumentNullException(nameof(project));

            this.generator = new CSJsonClassGenerator();
            this.generator.PropertyChanged += Generator_PropertyChanged;
            
            SelectedProject = project;
            SelectedNamespace = project.DefaultNamespace ?? project.AssemblyName ?? project.Namespaces.FirstOrDefault();
        }

        public JSConvertViewModel(CurioProject project, CSJsonClassGenerator generator) : this()
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (generator == null) throw new ArgumentNullException(nameof(generator));

            this.generator = generator;
            this.generator.PropertyChanged += Generator_PropertyChanged;
            SelectedProject = project;
            SelectedNamespace = project.DefaultNamespace ?? project.AssemblyName ?? project.Namespaces.FirstOrDefault();
        }

        private void Generator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OKCommand.QueryCanExecute();
        }

        public string SelectedNamespace
        {
            get => selNS;
            set
            {
                if (SetProperty(ref selNS, value))
                {
                    generator.Namespace = value;
                }
            }
        }

        public ObservableCollection<string> ActiveNamespaces
        {
            get => ns;
            protected set
            {
                SetProperty(ref ns, value);
            }
        }

        public bool RequestCanExecute(string commandId)
        {
            if (commandId == nameof(OKCommand))
            {
                return !generator.IsInvalid;
            }

            return true;
        }
    }
}

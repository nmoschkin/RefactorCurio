global using Community.VisualStudio.Toolkit;

global using Microsoft.VisualStudio.Shell;

global using System;

global using Task = System.Threading.Tasks.Task;

using CSRefectorCurio.ViewModels;

using DataTools.CSTools;

using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace CSRefectorCurio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(CurioExplorerToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.CSRefectorCurioString)]    
    public sealed class CSRefectorCurioPackage : ToolkitPackage
    {
        internal CurioExplorerViewModel curiovm;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            CurioExplorer.ServiceProvider = this;

            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();

           
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            curiovm = new CurioExplorerViewModel();

            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

            dte.Events.SolutionEvents.Opened += SolutionEvents_Opened;
            dte.Events.SolutionEvents.BeforeClosing += SolutionEvents_BeforeClosing;
        }

        private void SelectionEvents_OnChange()
        {
            var foo = "bar";
        }

        private void LoadProject()
        {
            _ = Task.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

                var ctrl = FindToolWindow();
                curiovm.Projects.Clear();

                foreach (EnvDTE.Project item in (IEnumerable)dte.ActiveSolutionProjects)
                {
                    if (item.FullName.ToLower().EndsWith(".csproj"))  
                        curiovm.Projects.Add(new ProjectReader(item.FullName));
                }

                ctrl.DataContext = curiovm;
            });
        }

        protected override async Task OnAfterPackageLoadedAsync(CancellationToken cancellationToken)
        {
            await base.OnAfterPackageLoadedAsync(cancellationToken);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

            if (dte.Solution.IsOpen)
            {
                LoadProject();
            }
        }

        private void SolutionEvents_BeforeClosing()
        {
            curiovm.Projects.Clear();
        }

        private void SolutionEvents_Opened()
        {
            LoadProject();
        }

        CurioExplorer FindToolWindow()
        {
            var twp = (CurioExplorerToolWindow.Pane)FindToolWindow(typeof(CurioExplorerToolWindow.Pane), 0, true);

            var ctrl = twp.Content as CurioExplorer;
            return ctrl;
        }


    }
}
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Microsoft.VisualStudio.Shell.Events;

global using System;

global using Task = System.Threading.Tasks.Task;

using CSRefectorCurio.Commands;
using CSRefectorCurio.ViewModels;

using DataTools.CSTools;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace CSRefectorCurio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideToolWindow(typeof(CurioExplorerToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.CSRefactorCurioString)]
    [ProvideUIContextRule(UIContextGuid,
    name: "Filter For Single Selection",
    expression: "SingleSel",
    termNames: new[] { "SingleSel" },
    termValues: new[] { "HierSingleSelectionName:^.+$" })]
    public sealed class CSRefectorCurioPackage : ToolkitPackage
    {
        internal CurioExplorerViewModel curiovm;
        
        public const string UIContextGuid = "17D7439F-90F8-4396-9B51-8309208381A5";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            CurioExplorer.ServiceProvider = this;

            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();
           
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            curiovm = new CurioExplorerViewModel();

            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));


            if (dte.Solution is object && dte.Solution.IsOpen)
            {
                LoadProject();
            }

            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterOpenSolution += HandleOpenSolution;

        }

        private void HandleOpenSolution(object sender, OpenSolutionEventArgs e)
        {
            LoadProject();            
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
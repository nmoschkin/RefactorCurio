global using Community.VisualStudio.Toolkit;

global using Microsoft.VisualStudio.Shell;
global using Microsoft.VisualStudio.Shell.Events;

global using System;

global using Task = System.Threading.Tasks.Task;

using CSRefactorCurio.ViewModels;

using DataTools.CSTools;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace CSRefactorCurio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    //[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideToolWindow(typeof(CurioExplorerToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.CSRefactorCurioString)]
    [ProvideUIContextRule(JsonItemContextGuid,
    name: "Filter For JSON Files",
    expression: "JsonSel",
    termNames: new[] { "JsonSel" },
    termValues: new[] { "HierSingleSelectionName:.json$" })]
    [ProvideUIContextRule(AddItemContextGuid,
    name: "Filter For Single Selection",
    expression: "SingleSel",
    termNames: new[] { "SingleSel" },
    termValues: new[] { "HierSingleSelectionName:^.+$" })]
    public sealed class CSRefectorCurioPackage : ToolkitPackage
    {
        static internal CurioExplorerViewModel CurioSolution;

        public const string AddItemContextGuid = "17D7439F-90F8-4396-9B51-8309208381A5";
        public const string JsonItemContextGuid = "CD497BC9-978B-4C88-A214-0E22886A9601";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            CurioSolution = new CurioExplorerViewModel();

            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

            if (dte.Solution is object && dte.Solution.IsOpen)
            {
                LoadProject();
            }

            dte.Events.SolutionEvents.Opened += SolutionEvents_Opened;
            dte.Events.SolutionEvents.BeforeClosing += SolutionEvents_BeforeClosing;
            dte.Events.SolutionItemsEvents.ItemAdded += SolutionItemsEvents_ItemAdded;
            dte.Events.SolutionItemsEvents.ItemRenamed += SolutionItemsEvents_ItemRenamed;
            dte.Events.SolutionItemsEvents.ItemRemoved += SolutionItemsEvents_ItemRemoved;
        }

        private void SolutionItemsEvents_ItemRemoved(EnvDTE.ProjectItem ProjectItem)
        {
            LoadProject();
        }

        private void SolutionItemsEvents_ItemRenamed(EnvDTE.ProjectItem ProjectItem, string OldName)
        {
            LoadProject();
        }

        private void SolutionItemsEvents_ItemAdded(EnvDTE.ProjectItem ProjectItem)
        {
            LoadProject();
        }

        protected override async Task OnAfterPackageLoadedAsync(CancellationToken cancellationToken)
        {
            await base.OnAfterPackageLoadedAsync(cancellationToken);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

            if (dte.Solution is object && dte.Solution.IsOpen)
            {
                LoadProject();
            }
        }

        EnvDTE.Solution currSln = null;

        private void LoadProject()
        {
            _ = Task.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

                if (dte.Solution == currSln) return;
                currSln = (EnvDTE.Solution)dte.Solution;

                CurioSolution.Projects.Clear();

                foreach (EnvDTE.Project item in (IEnumerable)dte.Solution.Projects)
                {
                    if (item.FullName.ToLower().EndsWith(".csproj"))
                    {                        
                        CurioSolution.Projects.Add(new CurioProject(item.FullName, item));
                    }
                }

                var ctrl = FindToolWindow();
                ctrl.DataContext = CurioSolution;
            });
        }

        private void SolutionEvents_BeforeClosing()
        {
            currSln = null;
            CurioSolution.Projects.Clear();
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
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
        static object syncRoot = new object();
        static CSRefectorCurioPackage _package;

        EnvDTE.Solution currSln = null;
        CurioExplorerSolution _curio;

        public static object SyncRoot => syncRoot;

        internal static CSRefectorCurioPackage Instance
        {
            get
            {
                lock (syncRoot)
                {
                    return _package;
                }
            }
            private set
            {
                lock (syncRoot)
                {
                    _package = value;
                }
            }
        }
        
        internal CurioExplorerSolution CurioSolution
        {
            get
            {
                lock (syncRoot)
                {
                    return _curio;
                }
            }
            private set
            {
                lock(syncRoot)
                {
                    _curio = value;
                }
            }
        }

        public const string AddItemContextGuid = "17D7439F-90F8-4396-9B51-8309208381A5";
        public const string JsonItemContextGuid = "CD497BC9-978B-4C88-A214-0E22886A9601";
       
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {

            CurioSolution = new CurioExplorerSolution();
            Instance = this;

            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

            if (dte.Solution is object && dte.Solution.IsOpen)
            {
                LoadProject();
            }

            await base.InitializeAsync(cancellationToken, progress);
        }

        protected override async Task OnAfterPackageLoadedAsync(CancellationToken cancellationToken)
        {
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterOpenSolution += SolutionEvents_OnAfterOpenSolution;
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnBeforeCloseSolution += SolutionEvents_OnBeforeCloseSolution;
            
            await base.OnAfterPackageLoadedAsync(cancellationToken);
        }

        private void SolutionEvents_OnAfterOpenSolution(object sender, OpenSolutionEventArgs e)
        {
            LoadProject();
        }

        private void SolutionEvents_OnBeforeCloseSolution(object sender, EventArgs e)
        {
            currSln = null;
            CurioSolution.Projects.Clear();
        }

        public async Task RefreshSolutionAsync(bool reload = true)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            lock (syncRoot)
            {
                EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

                if (!reload && dte.Solution == currSln) return;
                currSln = (EnvDTE.Solution)dte.Solution;
               
                CurioSolution.LoadFromDTE(dte);
            }
        }

        private void LoadProject()
        {            
            _ = RefreshSolutionAsync(false);
        }
    }
}
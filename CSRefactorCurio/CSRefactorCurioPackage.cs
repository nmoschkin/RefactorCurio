global using Community.VisualStudio.Toolkit;

global using Microsoft.VisualStudio.Shell;
global using Microsoft.VisualStudio.Shell.Events;

global using System;

global using Task = System.Threading.Tasks.Task;

using CSRefactorCurio.ViewModels;

using Microsoft.VisualStudio.PlatformUI;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

//Then you get an event
[assembly: InternalsVisibleTo("Experiments, PublicKey=00240000048000009400000006020000002400005253413100040000010001001da81ae298b1d5df1349b57367fe72036760259893883743e4b09bf45f490e3be7e3792a1943f57e226c058f06418bd40af347432e6ff2ba550c420d73be0c9fe748eac4314ff155399cc92851c7e783f01d484b15e4680d00ec6805b691b5ba7a3ecb1cd1e6ff727a67a98f0fa61dd591fe1d0b26a36fb8e9258d1863f9be96")]

namespace CSRefactorCurio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(CurioExplorerToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.CSRefactorCurioString)]
    // Register the options with this attribute on your package class:
    [ProvideOptionPage(typeof(Options.OptionsProvider.CSAppOptionsOptions), "Curio Refactor Studio", "JSON to C# Generation", 0, 0, true)]
    [ProvideProfile(typeof(Options.OptionsProvider.CSAppOptionsOptions), "Curio Refactor Studio", "JSON to C# Generation", 0, 0, true)]
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
    public sealed class CSRefactorCurioPackage : ToolkitPackage
    {
        private static object syncRoot = new object();
        private static CSRefactorCurioPackage _package;

        private EnvDTE.Solution currSln = null;
        private CurioExplorerSolution _curio;
        internal static ColorItems _colors;
        public static object SyncRoot => syncRoot;

        internal static CSRefactorCurioPackage Instance
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
                lock (syncRoot)
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

            EnvDTE.DTE dte = (EnvDTE.DTE)await GetServiceAsync(typeof(EnvDTE.DTE));

            if (dte != null && dte.Solution is object && dte.Solution.IsOpen)
            {
                LoadProject();
            }

            _colors = await ColorItems.CreateAsync(false);
            await base.InitializeAsync(cancellationToken, progress);
        }

        protected override async Task OnAfterPackageLoadedAsync(CancellationToken cancellationToken)
        {
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterOpenSolution += SolutionEvents_OnAfterOpenSolution;
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnBeforeCloseSolution += SolutionEvents_OnBeforeCloseSolution;

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;

            await base.OnAfterPackageLoadedAsync(cancellationToken);
        }

        private async void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            _colors = await ColorItems.CreateAsync(false);
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

            EnvDTE.DTE dte = (EnvDTE.DTE)await GetServiceAsync(typeof(EnvDTE.DTE));
            if (dte == null) return;

            lock (syncRoot)
            {
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
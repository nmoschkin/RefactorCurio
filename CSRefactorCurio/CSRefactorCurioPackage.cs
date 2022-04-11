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
            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            curiovm = new CurioExplorerViewModel();

            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

            if (dte.Solution is object && dte.Solution.IsOpen)
            {
                LoadProject();
            }

            dte.Events.SolutionEvents.Opened += SolutionEvents_Opened;
            dte.Events.SolutionEvents.BeforeClosing += SolutionEvents_BeforeClosing;

        }

        private void LoadProject()
        {
            _ = Task.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

                curiovm.Projects.Clear();

                foreach (EnvDTE.Project item in (IEnumerable)dte.Solution.Projects)
                {
                    if (item.FullName.ToLower().EndsWith(".csproj"))
                        curiovm.Projects.Add(new ProjectReader(item.FullName));
                }

                var ctrl = FindToolWindow();
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
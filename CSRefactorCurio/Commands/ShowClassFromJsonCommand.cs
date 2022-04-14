
using System.Windows.Forms;

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRefactorCurio.Forms;
using CSRefactorCurio.Dialogs;
using DataTools.CSTools;
using CSRefactorCurio.Helpers;

namespace CSRefactorCurio
{
    [Command(PackageIds.ShowClassFromJsonCommand)]
    internal sealed class ShowClassFromJsonCommand : BaseCommand<ShowClassFromJsonCommand>
    {
        public static ShowClassFromJsonCommand Instance { get; private set; }

        protected override Task InitializeCompletedAsync()
        {
            Instance = this;
            return base.InitializeCompletedAsync();
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (CSRefectorCurioPackage.Instance == null)
            {
                int t = 0;
                while (CSRefectorCurioPackage.Instance == null && t < 500)
                {
                    await Task.Delay(10);
                    t++;
                }
            }

            if (CSRefectorCurioPackage.Instance.CurioSolution.Solution == null)
            {
                await CSRefectorCurioPackage.Instance.RefreshSolutionAsync(true);
            }

            var dte = (EnvDTE.DTE)ToolkitPackage.GetGlobalService(typeof(EnvDTE.DTE));
            EnvDTE.ProjectItem selItem = null;
            EnvDTE.Project proj = null;
            
            string initPath = null;

            foreach (EnvDTE.SelectedItem o in dte.SelectedItems)
            {
                if (o.ProjectItem is EnvDTE.ProjectItem)
                {
                    selItem = o.ProjectItem;
                    initPath = (string)PropertyHelper.GetProperty(selItem, "FullPath");

                    if (selItem != null) break;
                }
                else if (o.Project is EnvDTE.Project)
                {
                    proj = o.Project;
                    break;
                }
            }

            if (proj == null) proj = selItem?.ContainingProject;

            CurioProject testproj = null;

            if (proj != null)
                testproj = (CurioProject)CSRefectorCurioPackage.Instance.CurioSolution.Projects.Where((p) => ((CurioProject)p).NativeProject.Equals(proj)).FirstOrDefault();

            JsonOptionsDialog dlg;
            
            if (testproj == null)
            {
                dlg = new JsonOptionsDialog();
            }
            else
            {
                if (initPath != null)
                {
                    dlg = new JsonOptionsDialog(testproj, initPath);
                }
                else
                {
                    dlg = new JsonOptionsDialog(testproj);
                }
            }

            dlg.ShowModal();

            //var frmJson = new JsonGeneratorForm(selItem);

            //if (frmJson.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{

            //}
        }
    }
}

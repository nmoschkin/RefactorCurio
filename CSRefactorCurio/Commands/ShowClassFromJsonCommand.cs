
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

namespace CSRefactorCurio
{
    [Command(PackageIds.ShowClassFromJsonCommand)]
    internal sealed class ShowClassFromJsonCommand : BaseCommand<ShowClassFromJsonCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = (EnvDTE.DTE)ToolkitPackage.GetGlobalService(typeof(EnvDTE.DTE));
            EnvDTE.ProjectItem selItem = null;
            EnvDTE.Project proj = null;

            foreach (EnvDTE.SelectedItem o in dte.SelectedItems)
            {
                if (o.Project is EnvDTE.Project)
                {
                    proj = o.Project;
                    break;
                }
                else
                {
                    selItem = o.ProjectItem;
                    if (selItem != null) break;
                }
            }

            if (proj == null) proj = selItem?.ContainingProject;

            CurioProject testproj = null;

            if (proj != null)
                testproj = CSRefectorCurioPackage.CurioSolution.Projects.Where((p) => p.NativeProject.Equals(proj)).FirstOrDefault();

            JsonOptionsDialog dlg;
            
            if (testproj == null)
            {
                dlg = new JsonOptionsDialog();
            }
            else
            {
                dlg = new JsonOptionsDialog(testproj);
            }

            dlg.ShowModal();

            //var frmJson = new JsonGeneratorForm(selItem);

            //if (frmJson.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{

            //}
        }
    }
}

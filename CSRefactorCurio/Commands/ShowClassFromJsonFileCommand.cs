
using System.Windows.Forms;

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRefactorCurio.Forms;

namespace CSRefactorCurio
{
    [Command(PackageIds.ShowClassFromJsonFileCommand)]
    internal sealed class ShowClassFromJsonFileCommand : BaseCommand<ShowClassFromJsonFileCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = (EnvDTE.DTE)ToolkitPackage.GetGlobalService(typeof(EnvDTE.DTE));
            EnvDTE.ProjectItem selItem = null;

            foreach (EnvDTE.SelectedItem o in dte.SelectedItems)
            {
                selItem = o.ProjectItem;
                break;
            }

            var frmJson = new JsonGeneratorForm(selItem);

            if (frmJson.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

            }
        }
    }
}

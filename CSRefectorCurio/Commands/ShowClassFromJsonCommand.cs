using CSRefectorCurio.Forms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRefectorCurio.Commands
{
    [Command(PackageIds.ShowClassFromJsonCommand)]
    internal sealed class ShowClassFromJsonCommand : BaseCommand<ShowClassFromJsonCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();            

            var dte = (EnvDTE.DTE)ToolkitPackage.GetGlobalService(typeof(EnvDTE.DTE));

            var selItem = dte.SelectedItems.Item(1);
            var frmJson = new PasteJSONForm(selItem);

            if (frmJson.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

            }
        }
    }
}

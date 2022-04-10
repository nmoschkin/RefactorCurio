using CSRefectorCurio.Forms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRefectorCurio.Commands
{
    [Command(PackageIds.ShowJSONPasterCommand)]
    internal sealed class ShowJSONPasterCommand : BaseCommand<ShowJSONPasterCommand>
    {

        public ShowJSONPasterCommand() : base()
        {

        }

        protected override async Task InitializeCompletedAsync()
        {
            await base.InitializeCompletedAsync();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = (EnvDTE.DTE)ToolkitPackage.GetGlobalService(typeof(EnvDTE.DTE));
            dte.Events.SelectionEvents.OnChange += SelectionEvents_OnChange;
        }

        private void SelectionEvents_OnChange()
        {
            var dte = (EnvDTE.DTE)ToolkitPackage.GetGlobalService(typeof(EnvDTE.DTE));

            var selItems = dte.SelectedItems;

            if (selItems.Count != 1)
            {
                this.Command.Enabled = false;
            }
            else
            {
                this.Command.Enabled = true;
            }
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();            

            var dte = (EnvDTE.DTE)ToolkitPackage.GetGlobalService(typeof(EnvDTE.DTE));
            
            var selItem = dte.SelectedItems.Item(0);
            var frmJson = new PasteJSONForm(selItem);

            if (frmJson.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

            }
        }
        
        
    }

}

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace CSRefactorCurio.Options
{
    [ComVisible(true)]
    [Guid("D8B47497-8AC9-4E2E-9D62-D8E8E7A47AA4")]
    public class FilterOptionsPage : UIElementDialogPage
    {
        protected override UIElement Child
        {
            get
            {
                FilterOptionsView page = new FilterOptionsView
                {
                    filterOptionsPage = this
                };
                page.Initialize();
                return page;
            }
        }
    }
}
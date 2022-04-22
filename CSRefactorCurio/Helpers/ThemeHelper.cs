using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CSRefactorCurio.Helpers
{
    internal static class ThemeHelper
    {

        public static void ApplyVSTheme(object item)
        {
            if (item is Panel p)
            {
                p.SetValue(Themes.UseVsThemeProperty, true);

                foreach (var child in p.Children)
                {
                    ApplyVSTheme(child);
                }
            }
            else if (item is ContentControl cc)
            {
                cc.SetValue(Themes.UseVsThemeProperty, true);

                if (cc.Content != null)
                {
                    ApplyVSTheme(cc.Content);
                }

            }
            else if (item is Control c)
            {
                c.SetValue(Themes.UseVsThemeProperty, true);

            }
            else if (item is IEnumerable e)
            {
                foreach (var item2 in e)
                {
                    ApplyVSTheme(item2);
                }
            }
        }
    }
}

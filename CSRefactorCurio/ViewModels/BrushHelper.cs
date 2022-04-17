using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace CSRefactorCurio.ViewModels
{
    internal static class BrushHelper
    {
        static IVsUIShell5 shell5;

        /// <summary>
        /// Gets a System.Drawing.Color value from the current theme for the given color key.
        /// </summary>
        /// <param name="vsUIShell">The IVsUIShell5 service, used to get the color's value.</param>
        /// <param name="themeResourceKey">The key to find the color for.</param>
        /// <returns>The current theme's value of the named color.</returns>
        public static System.Drawing.Color GetThemedGDIColor(this IVsUIShell5 vsUIShell, ThemeResourceKey themeResourceKey)
        {
            Validate.IsNotNull(vsUIShell, "vsUIShell");
            Validate.IsNotNull(themeResourceKey, "themeResourceKey");

            byte[] colorComponents = GetThemedColorRgba(vsUIShell, themeResourceKey);

            // Note: The Win32 color we get back from IVsUIShell5.GetThemedColor is ABGR
            return System.Drawing.Color.FromArgb(colorComponents[3], colorComponents[0], colorComponents[1], colorComponents[2]);
        }

        private static byte[] GetThemedColorRgba(IVsUIShell5 vsUIShell, ThemeResourceKey themeResourceKey)
        {
            Guid category = themeResourceKey.Category;
            __THEMEDCOLORTYPE colorType = __THEMEDCOLORTYPE.TCT_Foreground;
           if (themeResourceKey.KeyType == ThemeResourceKeyType.BackgroundColor || themeResourceKey.KeyType == ThemeResourceKeyType.BackgroundBrush)
            {
                colorType = __THEMEDCOLORTYPE.TCT_Background;
            }

            // This call will throw an exception if the color is not found
            uint rgbaColor = vsUIShell.GetThemedColor(ref category, themeResourceKey.Name, (uint)colorType);
            return BitConverter.GetBytes(rgbaColor);
        }
        public static Color GetThemedWPFColor(this IVsUIShell5 vsUIShell, ThemeResourceKey themeResourceKey)
        {
            Validate.IsNotNull(vsUIShell, "vsUIShell");
            Validate.IsNotNull(themeResourceKey, "themeResourceKey");
            
            return VsColors.GetThemedWPFColor(vsUIShell, themeResourceKey);


        }
        static BrushHelper()
        {
            IVsUIShell2 uiShell2 = CSRefectorCurioPackage.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell2;
            Debug.Assert(uiShell2 != null, "failed to get IVsUIShell2");
        }
    }
}

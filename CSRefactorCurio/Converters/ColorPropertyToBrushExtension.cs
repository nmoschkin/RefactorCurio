using DataTools.Graphics;

using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media;

namespace CSRefactorCurio.Converters
{
    internal enum ColorPropertyAspect
    {
        Foreground,
        Background
    }

    internal class ColorPropertyToBrushExtension : MarkupExtension
    {
        private static Dictionary<Color, SolidColorBrush> cache = new Dictionary<Color, SolidColorBrush>();
        private SolidColorBrush defaultBrush = new SolidColorBrush(Colors.White);

        private ColorPropertyAspect aspect = ColorPropertyAspect.Foreground;
        private bool isDark = false;

        public ColorPropertyToBrushExtension()
        {
            IVsUIShell2 uiShell2 = CSRefactorCurioPackage.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell2;

            if (uiShell2 != null)
            {
                //get the COLORREF structure
                uint win32Color;
                uiShell2.GetVSSysColorEx((int)__VSSYSCOLOREX3.VSCOLOR_WINDOW, out win32Color);
                var uc = new UniColor(win32Color);

                if (uc.V < 0.5)
                {
                    isDark = true;
                }
            }
        }

        public ColorPropertyAspect Aspect
        {
            get => aspect;
            set => aspect = value;
        }

        public string Key { get; set; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            SolidColorBrush retVal;

            if (Key is string s)
            {
                //IVsUIShell2 uiShell2 = CSRefactorCurioPackage.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell2;
                //Debug.Assert(uiShell2 != null, "failed to get IVsUIShell2");

                //if (uiShell2 != null)
                //{
                //    //get the COLORREF structure
                //    uint win32Color;
                //    uiShell2.GetVSSysColorEx((int), out win32Color);

                //    //translate it to a managed Color structure
                //}

                if (CSRefactorCurioPackage._colors.TryGetValue(s, out IColorableProperty c))
                {
                    Color chk;

                    if (aspect == ColorPropertyAspect.Foreground)
                    {
                        var uc = new UniColor(c.Foreground.ToArgb());

                        if (uc.V < 0.5 && isDark)
                        {
                            chk = c.Background;
                        }
                        else
                        {
                            chk = c.Foreground;
                        }
                    }
                    else
                    {
                        var uc = new UniColor(c.Background.ToArgb());

                        if (uc.V > 0.5 && isDark)
                        {
                            chk = c.Foreground;
                        }
                        else
                        {
                            chk = c.Background;
                        }
                    }

                    if (!cache.TryGetValue(chk, out retVal))
                    {
                        retVal = new SolidColorBrush(chk);
                        cache.Add(chk, retVal);
                    }

                    return retVal;
                }
            }

            return defaultBrush;
        }
    }
}
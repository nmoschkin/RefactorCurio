using DataTools.Text;

using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media;

namespace CSRefactorCurio.Converters
{
    public enum ColorPropertyAspect
    {
        Foreground,
        Background
    }

    internal class ColorPropertyToBrushExtension : MarkupExtension
    {
        private static Dictionary<Color, SolidColorBrush> cache = new Dictionary<Color, SolidColorBrush>();
        private SolidColorBrush defaultBrush = new SolidColorBrush(Colors.White);

        private ColorPropertyAspect aspect = ColorPropertyAspect.Foreground;

        public ColorPropertyToBrushExtension()
        {
            IVsUIShell2 uiShell2 = CSRefactorCurioPackage.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell2;
            Debug.Assert(uiShell2 != null, "failed to get IVsUIShell2");
            var s = TextTools.PrintFriendlySpeed(1209345843);
            if (uiShell2 != null)
            {
                //get the COLORREF structure
                uint win32Color;
                uiShell2.GetVSSysColorEx((int)__VSSYSCOLOREX3.VSCOLOR_WINDOW, out win32Color);
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
                    if (aspect == ColorPropertyAspect.Foreground)
                    {
                        if (!cache.TryGetValue(c.Foreground, out retVal))
                        {
                            retVal = new SolidColorBrush(c.Foreground);
                            cache.Add(c.Foreground, retVal);
                        }

                        return retVal;
                    }
                    else
                    {
                        if (!cache.TryGetValue(c.Background, out retVal))
                        {
                            retVal = new SolidColorBrush(c.Background);
                            cache.Add(c.Background, retVal);
                        }

                        return retVal;
                    }
                }
            }

            return defaultBrush;
        }
    }
}
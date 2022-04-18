using CSRefactorCurio.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
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

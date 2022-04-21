using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CSRefactorCurio.Converters
{
    internal class SelectionGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string s && int.TryParse(s, out int index) && value is int i)
            {
                return i == index;
            }
            else if (parameter is int i1 && value is int i3)
            {
                return i1 == i3;
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
            {
                if (parameter is int i) return i;
                else if (parameter is string s) return int.Parse(s);
            }

            throw new NotImplementedException();
        }
    }
}

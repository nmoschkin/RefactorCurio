using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CSRefactorCurio.Converters
{
    internal class BoolBrushConverter : IValueConverter
    {


        public Brush TrueBrush { get; set; } = Brushes.Green;

        public Brush FalseBrush { get; set; } = Brushes.Gray;


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                if (b) return TrueBrush;
                else return FalseBrush;
            }
            
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

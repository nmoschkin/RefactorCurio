﻿using CSRefactorCurio.Globalization.Resources;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace CSRefactorCurio.Converters
{
    internal class ItemCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                if (i == 0)
                {
                    return string.Format(AppResources.NO_OBJECTS);

                }
                else if (i == 1)
                {
                    return string.Format(AppResources.ONE_OBJECT);
                }
                else
                {
                    return string.Format(AppResources.X_OBJECTS, i);
                }
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

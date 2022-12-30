using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace CSRefactorCurio.Converters
{
    internal enum BoolConverterModes
    {
        Detect,
        InverseDetect,
        InverseBool,
        Number,
        InverseNumber,
        Visibility,
        InverseVisibility
    }

    internal class BoolConverter : IValueConverter
    {
        public BoolConverterModes Mode { get; set; } = BoolConverterModes.InverseBool;

        public Visibility HiddenVisibility { get; set; } = Visibility.Collapsed;

        public int NumberValue { get; set; } = 1;

        public int ConvertBackInverseValue { get; set; } = 0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (Mode)
            {
                case BoolConverterModes.InverseBool:
                    if (value is bool b) return !b;
                    else throw new InvalidCastException();

                case BoolConverterModes.Detect:
                    return value != null;

                case BoolConverterModes.InverseDetect:
                    return value == null;

                case BoolConverterModes.Number:

                    if (value is int i)
                    {
                        return (i == NumberValue);
                    }
                    break;

                case BoolConverterModes.InverseNumber:

                    if (value is int i2)
                    {
                        return (i2 != NumberValue);
                    }
                    break;

                case BoolConverterModes.Visibility:

                    if (value is bool b2)
                    {
                        if (b2 == true)
                        {
                            return Visibility.Visible;
                        }
                        else
                        {
                            return HiddenVisibility;
                        }
                    }
                    break;

                case BoolConverterModes.InverseVisibility:

                    if (value is bool b3)
                    {
                        if (b3 == false)
                        {
                            return Visibility.Visible;
                        }
                        else
                        {
                            return HiddenVisibility;
                        }
                    }
                    break;

                default:
                    return false;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (Mode)
            {
                case BoolConverterModes.Number:
                    if (value is bool b1 && b1) return NumberValue;
                    else return ConvertBackInverseValue;

                case BoolConverterModes.InverseNumber:
                    if (value is bool b2 && !b2) return ConvertBackInverseValue;
                    else return NumberValue;

                case BoolConverterModes.InverseBool:
                    if (value is bool b3) return !b3;
                    else throw new InvalidCastException();

                case BoolConverterModes.Visibility:

                    if (value is Visibility v1)
                    {
                        if (v1 == Visibility.Visible)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;

                case BoolConverterModes.InverseVisibility:

                    if (value is Visibility v2)
                    {
                        if (v2 == HiddenVisibility)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;

                default:
                    return false;
            }

            return false;
        }
    }
}
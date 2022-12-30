using DataTools.Code.Markers;

using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CSRefactorCurio.Converters
{
    internal class MarkerKindToMonikerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (value is MarkerKind kind)
            {

                switch (kind)
                {
                    case MarkerKind.Class:
                        return KnownMonikers.Class;

                    case MarkerKind.Enum:
                        return KnownMonikers.Enumeration;

                    case MarkerKind.Interface:
                        return KnownMonikers.Interface;

                    case MarkerKind.Property:
                        return KnownMonikers.Property;

                    case MarkerKind.Struct:
                        return KnownMonikers.Structure;

                    case MarkerKind.Delegate:
                        return KnownMonikers.Delegate;

                    case MarkerKind.Namespace:
                        return KnownMonikers.Namespace;

                    case MarkerKind.Event:
                        return KnownMonikers.Event;

                    case MarkerKind.Field:
                    case MarkerKind.FieldValue:
                        return KnownMonikers.Field;

                    case MarkerKind.Method:
                        return KnownMonikers.Method;

                    case MarkerKind.Constructor:
                        return KnownMonikers.NewClass;

                    case MarkerKind.Destructor:
                        return KnownMonikers.EndCall;

                    case MarkerKind.Const:
                        return KnownMonikers.Constant;

                    case MarkerKind.XMLDoc:
                        return KnownMonikers.XMLCommentTag;


                    case MarkerKind.Get:
                    case MarkerKind.Set:
                    case MarkerKind.Add:
                    case MarkerKind.Remove:
                        return KnownMonikers.Part;

                    case MarkerKind.LineComment:
                    case MarkerKind.BlockComment:
                        return KnownMonikers.Comment;
                }
            }

            return KnownMonikers.Item;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

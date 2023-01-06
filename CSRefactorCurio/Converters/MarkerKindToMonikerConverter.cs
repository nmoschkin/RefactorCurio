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
            
            if (value is CodeElementType kind)
            {

                switch (kind)
                {
                    case CodeElementType.Class:
                        return KnownMonikers.Class;

                    case CodeElementType.Enum:
                        return KnownMonikers.Enumeration;

                    case CodeElementType.Interface:
                        return KnownMonikers.Interface;

                    case CodeElementType.Property:
                        return KnownMonikers.Property;

                    case CodeElementType.Struct:
                        return KnownMonikers.Structure;

                    case CodeElementType.Delegate:
                        return KnownMonikers.Delegate;

                    case CodeElementType.Namespace:
                        return KnownMonikers.Namespace;

                    case CodeElementType.Event:
                        return KnownMonikers.Event;

                    case CodeElementType.Field:
                    case CodeElementType.FieldValue:
                        return KnownMonikers.Field;

                    case CodeElementType.Method:
                        return KnownMonikers.Method;

                    case CodeElementType.Constructor:
                        return KnownMonikers.NewClass;

                    case CodeElementType.Destructor:
                        return KnownMonikers.EndCall;

                    case CodeElementType.Const:
                        return KnownMonikers.Constant;

                    case CodeElementType.XMLDoc:
                        return KnownMonikers.XMLCommentTag;


                    case CodeElementType.Get:
                    case CodeElementType.Set:
                    case CodeElementType.Add:
                    case CodeElementType.Remove:
                        return KnownMonikers.Part;

                    case CodeElementType.LineComment:
                    case CodeElementType.BlockComment:
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

using DataTools.Essentials.Observable;

using Microsoft.Build.Framework;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace CSRefactorCurio.Helpers
{
    internal struct EnumListElement
    {
        public string Name { get; }

        public object Value { get; }

        public Type EnumType { get; }

        public EnumListElement(Type enumType, string name, object value)
        {
            if (!enumType.IsEnum) throw new ArgumentException(nameof(enumType));

            EnumType = enumType;
            Value = value;
            Name = name;
        }

        public EnumListElement(Type enumType, object value)
        {
            EnumType = enumType;
            Value = value;
            Name = "";

            var fields = enumType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            var bt = enumType.GetEnumUnderlyingType();

            foreach (var field in fields)
            {
                if (field.FieldType == enumType)
                {
                    if (value.Equals(field.GetValue(null)))
                    {
                        Name = field.Name;
                        break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    internal class EnumListElement<T> where T : Enum
    {
        public string Name { get; set; }

        public T Value { get; set; }

        public EnumListElement(string name, T value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    internal class EnumListExtension : MarkupExtension
    {
        private Type enumType;

        private EnumListProvider provider;

        [Required]
        public Type Type
        {
            get => enumType;
            set
            {
                if (enumType != value)
                {
                    enumType = value;
                    if (enumType != null)
                    {
                        provider = new EnumListProvider(enumType);
                    }
                }
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (provider == null || enumType == null || !enumType.IsEnum) throw new InvalidOperationException();
            return provider.Items;
        }
    }

    internal class EnumListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EnumListElement elem)
            {
                if (elem.EnumType == targetType)
                {
                    return elem.Value;
                }
            }
            else if (value.GetType().IsEnum)
            {
                return new EnumListElement(value.GetType(), value);
            }

            throw new InvalidOperationException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType().IsEnum)
            {
                return new EnumListElement(value.GetType(), value);
            }
            else if (value is EnumListElement elem)
            {
                if (elem.EnumType == targetType)
                {
                    return elem.Value;
                }
            }

            throw new InvalidOperationException();
        }
    }

    internal class EnumListProvider : ObservableBase
    {
        private List<EnumListElement> items;
        private EnumListElement selItem;
        private Type enumType;

        public List<EnumListElement> Items => items;

        public Type EnumType
        {
            get => enumType;
            set
            {
                if (SetProperty(ref enumType, value))
                {
                    InitEnum(enumType);
                }
            }
        }

        public EnumListElement SelectedItem
        {
            get => selItem;
            set
            {
                SetProperty(ref selItem, value);
            }
        }

        public EnumListProvider(Type enumType)
        {
            InitEnum(enumType);
        }

        private void InitEnum(Type enumType)
        {
            if (!enumType.IsEnum) throw new ArgumentException(nameof(enumType));
            items = new List<EnumListElement>();

            var fields = enumType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            var bt = enumType;

            foreach (var field in fields)
            {
                if (field.FieldType == bt)
                {
                    items.Add(new EnumListElement(enumType, field.Name, field.GetValue(null)));
                }
            }
        }
    }

    internal class EnumListProvider<T> : ObservableBase where T : Enum
    {
        private List<EnumListElement<T>> items = new List<EnumListElement<T>>();
        private EnumListElement<T> selItem;

        public List<EnumListElement<T>> Items => items;

        public EnumListElement<T> SelectedItem
        {
            get => selItem;
            set
            {
                SetProperty(ref selItem, value);
            }
        }

        public EnumListProvider()
        {
            var fields = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            var bt = typeof(T);

            foreach (var field in fields)
            {
                if (field.FieldType == bt)
                {
                    items.Add(new EnumListElement<T>(field.Name, (T)field.GetValue(null)));
                }
            }
        }
    }
}
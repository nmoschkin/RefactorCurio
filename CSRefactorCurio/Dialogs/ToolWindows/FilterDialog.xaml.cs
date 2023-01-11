using CSRefactorCurio.Globalization.Resources;

using DataTools.Code.CS.Filtering;
using DataTools.Essentials.Converters.EnumDescriptions.Framework;

using Microsoft.VisualStudio.PlatformUI;

using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace CSRefactorCurio.Dialogs
{
    public partial class FilterDialog : DialogWindow
    {
        private CodeFilterOptions options;

        public FilterDialog()
        {
            InitializeComponent();
        }

        internal FilterDialog(CodeFilterOptions options = null) : this()
        {
            this.options = options ?? new CodeFilterOptions();

            var props = typeof(CodeFilterOptions).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            int row = 0;
            foreach (var prop in props)
            {
                var elem = CreateElementForType(prop.PropertyType, propertyName: prop.Name);

                if (elem != null)
                {
                    ControlGrid.Children.Add(elem);
                    ControlGrid.RowDefinitions.Add(new RowDefinition());
                    elem.SetValue(Grid.RowProperty, row++);

                    var obj = prop.GetValue(this.options);

                    if ((prop.PropertyType == typeof(bool?) || prop.PropertyType == typeof(bool)) && elem is CheckBox cb)
                    {
                        if (obj == null) cb.IsChecked = null;
                        else cb.IsChecked = (bool)obj;
                    }
                    else if (elem is TextBox tb)
                    {
                        tb.Text = (string)obj;
                    }
                    else if (elem is Grid gr && prop.PropertyType.IsEnum)
                    {
                        if (gr.Children[0] is RadioButton)
                        {
                            foreach (var uie in gr.Children)
                            {
                                if (uie is RadioButton crad)
                                {
                                    if (crad?.Equals(obj) ?? false)
                                    {
                                        crad.IsChecked = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var ot = obj.ToString().Trim();

                            foreach (var uie in gr.Children)
                            {
                                if (uie is CheckBox chk && chk.Tag is Enum ec)
                                {
                                    if (ot.Contains(ec.ToString()))
                                    {
                                        chk.IsChecked = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static Type GetTypeFromNullable(Type type)
        {
            if (type.Name.ToLower().StartsWith("null") && type.IsGenericType) return type.GetGenericArguments()[0];
            return type;
        }

        public static UIElement CreateElementForType(Type type, int maxcols = 2, string propertyName = null)
        {
            if (maxcols < 1) maxcols = 1;

            propertyName ??= type.Name;

            if (type == typeof(string))
            {
                return new TextBox();
            }
            else if (type == typeof(bool) || type == typeof(bool?))
            {
                var obj = new CheckBox();

                obj.IsThreeState = type == typeof(bool?);
                obj.Content = AppResources.ResourceManager.GetString(propertyName) ?? propertyName;
                obj.Tag = type;

                return obj;
            }

            type = GetTypeFromNullable(type);

            if (type.IsEnum)
            {
                var g = new Grid();

                var values = type.GetFields(BindingFlags.Static | BindingFlags.Public);

                int i, c = values.Length;

                for (i = 0; i < maxcols; i++)
                {
                    g.ColumnDefinitions.Add(new ColumnDefinition());
                }

                int cc = 0, cr = 0;

                if (type.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    var cb = new ComboBox();
                    var l = new List<DescribedEnum>();

                    for (i = 0; i < c; i++)
                    {
                        var eval = (Enum)values[i].GetValue(null);
                        l.Add(new DescribedEnum(eval));
                    }

                    cb.ItemsSource = l;
                    return cb;
                }
                else
                {
                    for (i = 0; i < c; i++)
                    {
                        UIElement newobj;

                        var eval = (Enum)values[i].GetValue(null);
                        var apptry = EnumInfo.GetEnumName(eval);

                        if (cc == 0)
                        {
                            g.RowDefinitions.Add(new RowDefinition());
                        }

                        newobj = new CheckBox()
                        {
                            Content = apptry,
                            Tag = new DescribedEnum(eval)
                        };

                        newobj.SetValue(Grid.ColumnProperty, cc);
                        newobj.SetValue(Grid.RowProperty, cr);

                        g.Children.Add(newobj);
                    }

                    cc++;

                    if (cc >= maxcols)
                    {
                        cc = 0;
                        cr++;
                    }
                }

                return g;
            }

            return new TextBlock();
        }
    }
}
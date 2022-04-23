using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSRefactorCurio.Controls
{

    public enum ButtonSide
    {
        Left,
        Right,
    }


    /// <summary>
    /// Interaction logic for SortOrderControl.xaml
    /// </summary>
    public partial class SortOrderControl : UserControl
    {

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(SortOrderControl), new PropertyMetadata(0, OnSelectedIndexChanged));

        private static void OnSelectedIndexChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is SortOrderControl owner)
            {
                if (e.NewValue is int value)
                {
                    if (owner.ListArea.SelectedIndex != value)
                    {
                        owner.ListArea.SelectedIndex = value;
                    }
                }
            }
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(SortOrderControl), new PropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is SortOrderControl owner)
            {
                if (e.NewValue is object value)
                {
                    if (owner.ListArea.SelectedItem != value)
                    {
                        owner.ListArea.SelectedItem = value;
                    }
                }
            }
        }

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IList), typeof(SortOrderControl), new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is SortOrderControl owner)
            {
                if (e.NewValue is IList value)
                {
                    var lnv = new List<object>();

                    // make sure each item is unique
                    foreach (object item in value)
                    {
                        if (lnv.Contains(item))
                        {
                            throw new DuplicateNameException();
                        }

                        lnv.Add(item);
                    }
                }
            }
        }

        public ButtonSide ButtonSide
        {
            get { return (ButtonSide)GetValue(ButtonSideProperty); }
            set { SetValue(ButtonSideProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonSide.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonSideProperty =
            DependencyProperty.Register("ButtonSide", typeof(ButtonSide), typeof(SortOrderControl), new PropertyMetadata(ButtonSide.Right, OnButtonSideChanged));

        private static void OnButtonSideChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is SortOrderControl owner)
            {
                if (e.NewValue is ButtonSide value)
                {
                    owner.DoLayout(value);
                }
            }
        }

        public SortOrderControl()
        {
            InitializeComponent();
        }

        private void DoLayout(ButtonSide side)
        {
            if (side == ButtonSide.Left)
            {
                ButtonsArea.SetValue(Grid.ColumnProperty, 0);
            }
            else
            {
                ButtonsArea.SetValue(Grid.ColumnProperty, 2);
            }
        }

        private void ListArea_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListArea.SelectedItem != SelectedItem)
            {
                SelectedItem = ListArea.SelectedItem;
            }

            if (ListArea.SelectedIndex != SelectedIndex)
            {
                SelectedIndex = ListArea.SelectedIndex;
            }

            DownButton.IsEnabled = SelectedItem != null && (ListArea.SelectedIndex > 0);
            UpButton.IsEnabled = SelectedItem != null && (ListArea.SelectedIndex < ListArea.Items.Count - 1);
        }

        private void MoveDown()
        {
            var x = ListArea.SelectedIndex;

            if (x < ListArea.Items.Count - 1)
            {
                var oi = ListArea.Items[x + 1];
                ListArea.Items[x + 1] = ListArea.Items[x];
                ListArea.Items[x] = oi;
                ListArea.SelectedIndex += 1;

                ItemsSource[x + 1] = ListArea.Items[x];
                ItemsSource[x] = oi;
            }

        }
        private void MoveUp()
        {
            var x = ListArea.SelectedIndex;

            if (x > 0)
            {
                var oi = ListArea.Items[x - 1];

                ListArea.Items[x - 1] = ListArea.Items[x];
                ListArea.Items[x] = oi;
                ListArea.SelectedIndex -= 1;

                ItemsSource[x - 1] = ListArea.Items[x];
                ItemsSource[x] = oi;

            }

        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            MoveUp();
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            MoveDown();
        }
    }
}

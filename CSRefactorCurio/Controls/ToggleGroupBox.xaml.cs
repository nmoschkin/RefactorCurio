using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSRefactorCurio.Controls
{
    /// <summary>
    /// Interaction logic for ToggleGroupBox.xaml
    /// </summary>
    public partial class ToggleGroupBox : UserControl
    {
        public ToggleGroupBox()
        {
            InitializeComponent();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (newContent is Grid g)
            {
                ReSeat(g);
            }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ToggleGroupBox), new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        private static void OnOrientationChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ToggleGroupBox owner)
            {
                if (e.NewValue is Orientation value && owner.Content is Grid g)
                {
                    owner.ReSeat(g);
                }
            }
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ToggleGroupBox), new PropertyMetadata(-1, OnSelectedIndexChanged));

        private static void OnSelectedIndexChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ToggleGroupBox owner)
            {
                if (e.NewValue is int value)
                {
                    owner.SetIndex(value);
                }
            }
        }

        private void SetIndex(int index)
        {
            if (Content is Grid g)
            {
                int i, c = g.Children.Count;

                for (i = 0; i < c; i++)
                {
                    if (i == index)
                    {
                        ((ToggleButton)g.Children[i]).IsChecked = true;
                    }
                    else
                    {
                        ((ToggleButton)g.Children[i]).IsChecked = false;
                    }
                }
            }
        }

        private void ReSeat(Grid Items)
        {
            Items.RowDefinitions.Clear();
            Items.ColumnDefinitions.Clear();

            if (Orientation == Orientation.Horizontal)
            {
                int i, c = Items.Children.Count;

                for (i = 0; i < c; i++)
                {
                    Items.ColumnDefinitions.Add(new ColumnDefinition());
                    Items.Children[i].SetValue(Grid.ColumnProperty, i);
                    Items.Children[i].SetValue(Grid.RowProperty, 0);
                }
            }
            else
            {
                int i, c = Items.Children.Count;

                for (i = 0; i < c; i++)
                {
                    Items.RowDefinitions.Add(new RowDefinition());

                    Items.Children[i].SetValue(Grid.RowProperty, i);
                    Items.Children[i].SetValue(Grid.ColumnProperty, 0);
                }
            }
        }
    }

}

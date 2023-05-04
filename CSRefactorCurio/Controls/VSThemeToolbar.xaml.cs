using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace CSRefactorCurio.Controls
{
    /// <summary>
    /// Interaction logic for VSThemeToolbar.xaml
    /// </summary>
    [ContentProperty(nameof(Items))]
    public partial class VSThemeToolbar : UserControl
    {
        public VSThemeToolbar()
        {
            InitializeComponent();            
            MainBar.Loaded += ToolBar_Loaded;           
        }        

        public ItemCollection Items
        {
            get => MainBar.Items;
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(VSThemeToolbar), new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is VSThemeToolbar owner)
            {
                if (e.NewValue is IEnumerable value)
                {
                    owner.SetupSource(value);
                }
            }
        }

        private void SetupSource(IEnumerable source)
        {
            if (source == null)
            {

            }
        }






        //public object SelectedItem
        //{
        //    get { return (object)GetValue(SelectedItemProperty); }
        //    set { SetValue(SelectedItemProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty SelectedItemProperty =
        //    DependencyProperty.Register("SelectedItem", typeof(object), typeof(VSThemeToolbar), new PropertyMetadata(null, OnSelectedItemChanged));

        //private static void OnSelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if (sender is VSThemeToolbar owner)
        //    {
        //        if (e.NewValue is object value)
        //        {
        //            owner.ExternalSetSelected(value);
        //        }
        //    }
        //}

        //private void ExternalSetSelected(object value)
        //{

        //}

        //private void InternalSetSelected(object value)
        //{

        //}

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }

            //var props = typeof(VsBrushes).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            //var samctl = new Label();

            //foreach (var p in props)
            //{
            //    object obj;
            //    var s = (string)p.GetValue(null);

            //    samctl.SetResourceReference(Label.BackgroundProperty, s);

            //    if (samctl.Background is SolidColorBrush br)
            //    {
            //        brushkeys.Add(p.Name, br.Color.ToString());
            //    }
            //}

            //Clipboard.SetText(JsonConvert.SerializeObject(brushkeys));
        }
    }
}

using CSRefactorCurio.Helpers;

using DataTools.Code.Project.Properties;
using DataTools.Essentials.Observable;

using System.Windows.Media;

namespace CSRefactorCurio
{
    /// <summary>
    /// Wraps <see cref="EnvDTE.ColorableItems"/>
    /// </summary>
    internal class ColorableProperty : ObservableBase, IColorableProperty
    {
        protected EnvDTE.ColorableItems _native;
        protected PropertiesContainer _container = null;

        public EnvDTE.ColorableItems NativeObject => _native;

        public IPropertiesContainer Container { get; }

        public string Name => _native.Name;

        public Color Background
        {
            get => BrushHelper.MakeColorFromNumber(_native.Background | 0xff000000);
        }

        public bool Bold
        {
            get => _native.Bold;
        }

        public Color Foreground
        {
            get => BrushHelper.MakeColorFromNumber(_native.Foreground | 0xff000000);
        }

        public object Value { get; set; }

        public ColorableProperty(IPropertiesContainer parent, EnvDTE.ColorableItems native)
        {
            _native = native;

            Container = parent;
        }
    }
}
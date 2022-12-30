using DataTools.Code.Project.Properties;
using DataTools.Essentials.Observable;

namespace CSRefactorCurio
{
    /// <summary>
    /// Wraps <see cref="EnvDTE.Property"/>
    /// </summary>
    internal class Property : ObservableBase, IProperty
    {
        protected EnvDTE.Property _native;
        protected PropertiesContainer _container = null;

        public EnvDTE.Property NativeObject => _native;

        public IPropertiesContainer Container { get; }

        public string Name => _native.Name;

        public virtual object Value
        {
            get
            {
                if (_native == null) return null;
                if (_native.Value is EnvDTE.Properties prn)
                {
                    if (_container == null)
                    {
                        _container = new PropertiesContainer(prn);
                    }

                    return _container;
                }

                return _native.Value;
            }
            set
            {
                if (_native.Value != value)
                {
                    _native.Value = value;
                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(Name);
                }
            }
        }

        public Property(IPropertiesContainer parent, EnvDTE.Property native)
        {
            _native = native;
            Container = parent;
        }
    }
}
using DataTools.Code.Project.Properties;

namespace CSRefactorCurio
{
    internal class PropertiesContainer : PropertiesContainerBase<EnvDTE.Properties, IProperty>
    {
        public PropertiesContainer(object nativeObj) : base(nativeObj, false)
        {
        }

        public override object Parent => _native.Parent;

        public override void Refresh()
        {
            foreach (var oldprop in _properties)
            {
                oldprop.Value.PropertyChanged -= OnChildPropertyChanged;
            }
            _properties.Clear();

            foreach (EnvDTE.Property prop in _native)
            {
                var newprop = new Property(this, prop);
                newprop.PropertyChanged += OnChildPropertyChanged;

                _properties.Add(prop.Name, newprop);
                OnPropertyChanged(prop.Name);
            }
        }
    }
}
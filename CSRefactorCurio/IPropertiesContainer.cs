using DataTools.Observable;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSRefactorCurio
{
    internal interface IProperty : INotifyPropertyChanged
    {
        IPropertiesContainer Container { get; }
        
        string Name { get; }

        object Value { get; set; }
    }

    internal interface IPropertiesContainer : INotifyPropertyChanged, IEnumerable<IProperty>
    {
        object Parent { get; }

        IProperty this[string name] { get; }

    }

    internal class Property : ObservableBase, IProperty
    {
        EnvDTE.Property _native;

        public EnvDTE.Property NativeObject => _native;

        public IPropertiesContainer Container { get; }

        public string Name => _native.Name;
        
        public object Value
        {
            get => _native.Value;
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
    
        internal Property(IPropertiesContainer parent, EnvDTE.Property native)
        {
            _native = native;
            Container = parent;
        }
    
    }

    internal class PropertiesContainer : ObservableBase, IPropertiesContainer
    {
        EnvDTE.Properties _native;

        private Dictionary<string, IProperty> _properties = new Dictionary<string, IProperty>();

        public EnvDTE.Properties NativeObject => _native;

        public object Parent => _native.Parent;

        protected Dictionary<string, IProperty> Properties => _properties;

        public virtual IProperty this[string name]
        {
            get => _properties[name];
        }

        public PropertiesContainer(EnvDTE.Solution sln)
        {
            _native = sln.Properties;
            Refresh();
        }

        public PropertiesContainer(EnvDTE.Project project)
        {
            _native = project.Properties;
            Refresh();
        }

        public PropertiesContainer(EnvDTE.ProjectItem item)
        {
            _native = item.Properties;
            Refresh();
        }

        public PropertiesContainer(EnvDTE.Properties native)
        {
            _native = native;
            Refresh();
        }

        public virtual void Refresh()
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

        private void OnChildPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IProperty.Value)) OnPropertyChanged(e.PropertyName);
        }

        public IEnumerator<IProperty> GetEnumerator()
        {
            foreach (var kv in _properties)
            {
                yield return kv.Value;
            }

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var kv in _properties)
            {
                yield return kv.Value;
            }

            yield break;
        }
    }

}

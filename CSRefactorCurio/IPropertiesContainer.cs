using CSRefactorCurio.Helpers;

using DataTools.Essentials.Observable;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CSRefactorCurio
{
    /// <summary>
    /// Simple interface for the EnvDTE properties.
    /// </summary>
    internal interface IProperty : INotifyPropertyChanged
    {
        IPropertiesContainer Container { get; }

        string Name { get; }

        object Value { get; set; }
    }

    /// <summary>
    /// Simple interface for the colorable property.
    /// </summary>
    internal interface IColorableProperty : IProperty
    {
        Color Background { get; }

        bool Bold { get; }

        Color Foreground { get; }
    }

    /// <summary>
    /// Simple interface for a property container.
    /// </summary>
    internal interface IPropertiesContainer
    {
        object Parent { get; }
    }

    /// <summary>
    /// Simple interface for a property container.
    /// </summary>
    internal interface IPropertiesContainer<T> : IPropertiesContainer, INotifyPropertyChanged, IEnumerable<T> where T : IProperty
    {
        T this[string name] { get; }
    }

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

    internal class PropertiesContainer : PropertiesContainer<EnvDTE.Properties, IProperty>
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

    internal class ColorItems : PropertiesContainer<EnvDTE.FontsAndColorsItems, IColorableProperty>
    {
        private EnvDTE.DTE dte;

        public static async Task<ColorItems> CreateAsync(bool lazy = true)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = (EnvDTE.DTE)CSRefactorCurioPackage.GetGlobalService(typeof(EnvDTE.DTE));

            var props = new PropertiesContainer(dte.get_Properties("FontsAndColors", "TextEditor"));
            return new ColorItems(((Property)props["FontsAndColorsItems"]).NativeObject.Object, lazy);
        }

        public static ColorItems Create(bool lazy = true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = (EnvDTE.DTE)CSRefactorCurioPackage.GetGlobalService(typeof(EnvDTE.DTE));

            var props = new PropertiesContainer(dte.get_Properties("FontsAndColors", "TextEditor"));
            return new ColorItems(((Property)props["FontsAndColorsItems"]).NativeObject.Object, lazy);
        }

        public ColorItems(object nativeObj, bool lazy) : base(nativeObj, lazy)
        {
            dte = (EnvDTE.DTE)CSRefactorCurioPackage.GetGlobalService(typeof(EnvDTE.DTE));
            _native = (EnvDTE.FontsAndColorsItems)nativeObj;
        }

        public override object Parent => dte;

        public override IColorableProperty this[string name]
        {
            get => GetCreateSingleItem(name);
        }

        public virtual void RefreshCurrent()
        {
            var keys = Keys.ToList();

            foreach (var key in keys)
            {
                if (_properties.TryGetValue(key, out var item))
                {
                    item.PropertyChanged -= OnChildPropertyChanged;

                    var prop = _native.Item(key) as EnvDTE.ColorableItems;
                    var newprop = new ColorableProperty(this, prop);
                    _properties[key] = newprop;
                    newprop.PropertyChanged += OnChildPropertyChanged;
                    OnPropertyChanged(prop.Name);
                }
            }
        }

        public override void Refresh()
        {
            foreach (var oldprop in _properties)
            {
                oldprop.Value.PropertyChanged -= OnChildPropertyChanged;
            }
            _properties.Clear();

            foreach (EnvDTE.ColorableItems prop in _native)
            {
                var newprop = new ColorableProperty(this, prop);
                newprop.PropertyChanged += OnChildPropertyChanged;

                _properties.Add(prop.Name, newprop);
                OnPropertyChanged(prop.Name);
            }
        }

        public override bool TryGetValue(string key, out IColorableProperty value)
        {
            value = GetCreateSingleItem(key);
            return value != null;
        }

        protected IColorableProperty GetCreateSingleItem(string key)
        {
            var prop = _native.Item(key) as EnvDTE.ColorableItems;

            if (_properties.TryGetValue(prop.Name, out var item))
            {
                return item;
            }

            if (prop != null)
            {
                var newprop = new ColorableProperty(this, prop);
                newprop.PropertyChanged += OnChildPropertyChanged;

                _properties.Add(prop.Name, newprop);
                OnPropertyChanged(prop.Name);
                return newprop;
            }

            return null;
        }
    }

    /// <summary>
    /// Wraps <see cref="EnvDTE.Properties"/>
    /// </summary>
    internal abstract class PropertiesContainer<TNative, TWrap> : ObservableBase, IPropertiesContainer<TWrap>, IReadOnlyDictionary<string, TWrap> where TNative : IEnumerable where TWrap : IProperty
    {
        protected bool _lazy;

        protected TNative _native;

        protected Dictionary<string, TWrap> _properties = new Dictionary<string, TWrap>();

        protected Dictionary<string, TWrap> Properties => _properties;

        public virtual TNative NativeObject => _native;

        public abstract object Parent { get; }

        public virtual IEnumerable<string> Keys => ((IReadOnlyDictionary<string, TWrap>)_properties).Keys;

        public virtual IEnumerable<TWrap> Values => ((IReadOnlyDictionary<string, TWrap>)_properties).Values;

        public virtual int Count => ((IReadOnlyCollection<KeyValuePair<string, TWrap>>)_properties).Count;

        public virtual bool IsLazyLoad
        {
            get => _lazy;
            protected set => _lazy = value;
        }

        public virtual TWrap this[string name]
        {
            get => _properties[name];
        }

        protected PropertiesContainer(bool lazy)
        {
            _lazy = lazy;
        }

        public PropertiesContainer(object nativeObj, bool lazy)
        {
            _lazy = lazy;

            if (nativeObj is TNative native)
            {
                _native = native;
            }
            else if (typeof(TNative) == typeof(EnvDTE.Properties))
            {
                if (nativeObj is EnvDTE.Solution sln)
                {
                    _native = (TNative)sln.Properties;
                }
                else if (nativeObj is EnvDTE.Project project)
                {
                    _native = (TNative)project.Properties;
                }
                else if (nativeObj is EnvDTE.ProjectItem item)
                {
                    _native = (TNative)item.Properties;
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (!_lazy) Refresh();
        }

        public abstract void Refresh();

        protected virtual void OnChildPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IProperty.Value)) OnPropertyChanged(e.PropertyName);
        }

        public virtual IEnumerator<TWrap> GetEnumerator()
        {
            if (_lazy && _properties.Count == 0) Refresh();

            foreach (var kv in _properties)
            {
                yield return kv.Value;
            }

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual bool ContainsKey(string key)
        {
            return ((IReadOnlyDictionary<string, TWrap>)_properties).ContainsKey(key);
        }

        public virtual bool TryGetValue(string key, out TWrap value)
        {
            return ((IReadOnlyDictionary<string, TWrap>)_properties).TryGetValue(key, out value);
        }

        IEnumerator<KeyValuePair<string, TWrap>> IEnumerable<KeyValuePair<string, TWrap>>.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, TWrap>>)_properties).GetEnumerator();
        }
    }
}
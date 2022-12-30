using DataTools.Code.Project.Properties;

using System.Linq;
using System.Threading.Tasks;

namespace CSRefactorCurio
{
    internal class ColorItems : PropertiesContainerBase<EnvDTE.FontsAndColorsItems, IColorableProperty>
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
}
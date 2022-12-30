using DataTools.Code.Project.Properties;
using DataTools.Essentials.Observable;

using System.Collections;
using System.Collections.Generic;

namespace CSRefactorCurio
{
    /// <summary>
    /// Wraps <see cref="EnvDTE.Properties"/>
    /// </summary>
    internal abstract class PropertiesContainerBase<TNative, TWrap> : ObservableBase, IPropertiesContainer<TWrap>, IReadOnlyDictionary<string, TWrap> where TNative : IEnumerable where TWrap : IProperty
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

        protected PropertiesContainerBase(bool lazy)
        {
            _lazy = lazy;
        }

        public PropertiesContainerBase(object nativeObj, bool lazy)
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
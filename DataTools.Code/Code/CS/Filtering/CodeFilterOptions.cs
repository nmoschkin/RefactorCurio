using DataTools.Code.Markers;
using DataTools.Code.Project;
using DataTools.Essentials.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataTools.Code.CS.Filtering
{
    /// <summary>
    /// Code Filter Options
    /// </summary>
    /// <remarks>
    /// Nullable elements for indicating filter validity.
    /// </remarks>
    public class CodeFilterOptions : ICodeElement, ICloneable
    {
        protected static readonly PropertyInfo[] codeFilterProps = typeof(CodeFilterOptions).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        protected static readonly PropertyInfo[] codeElementProps = typeof(ICodeElement).GetProperties();
        public virtual List<string> Attributes { get; set; } = null;
        public virtual string DataType { get; set; } = null;
        public virtual string Generics { get; set; } = null;
        public virtual IImportInfo ImportInfo { get; set; } = null;
        public virtual List<string> Inheritances { get; set; } = null;
        public virtual string InheritanceString { get; set; } = null;
        public virtual bool? IsAbstract { get; set; } = null;
        public virtual bool? IsAsync { get; set; } = null;
        public virtual bool? IsExplicit { get; set; } = null;
        public virtual bool? IsExtern { get; set; } = null;
        public virtual bool? IsImplicit { get; set; } = null;
        public virtual bool? IsNew { get; set; } = null;
        public virtual bool? IsOverride { get; set; } = null;
        public virtual bool? IsPartial { get; set; } = null;
        public virtual bool? IsReadOnly { get; set; } = null;
        public virtual bool? IsRef { get; set; } = null;
        public virtual bool? IsSealed { get; set; } = null;
        public virtual bool? IsStatic { get; set; } = null;
        public virtual bool? IsUnsafe { get; set; } = null;
        public virtual bool? IsVirtual { get; set; } = null;
        public virtual MarkerKind? Kind { get; set; } = null;
        public virtual AccessModifiers? AccessModifiers { get; set; } = null;
        public virtual List<string> MethodParams { get; set; } = null;
        public virtual string MethodParamsString { get; set; } = null;
        public virtual string Name { get; set; } = null;
        public virtual string WhereClause { get; set; } = null;

        bool ICodeElement.IsAbstract
        {
            get => IsAbstract ?? false;
            set => IsAbstract = value;
        }

        bool ICodeElement.IsAsync
        {
            get => IsAsync ?? false;
            set => IsAsync = value;
        }

        bool ICodeElement.IsExplicit
        {
            get => IsExplicit ?? false;
            set => IsExplicit = value;
        }

        bool ICodeElement.IsExtern
        {
            get => IsExtern ?? false;
            set => IsExtern = value;
        }

        bool ICodeElement.IsImplicit
        {
            get => IsImplicit ?? false;
            set => IsImplicit = value;
        }

        bool ICodeElement.IsNew
        {
            get => IsNew ?? false;
            set => IsNew = value;
        }

        bool ICodeElement.IsOverride
        {
            get => IsOverride ?? false;
            set => IsOverride = value;
        }

        bool ICodeElement.IsPartial
        {
            get => IsPartial ?? false;
            set => IsPartial = value;
        }

        bool ICodeElement.IsReadOnly
        {
            get => IsReadOnly ?? false;
            set => IsReadOnly = value;
        }

        bool ICodeElement.IsRef
        {
            get => IsRef ?? false;
            set => IsRef = value;
        }

        bool ICodeElement.IsSealed
        {
            get => IsSealed ?? false;
            set => IsSealed = value;
        }

        bool ICodeElement.IsStatic
        {
            get => IsStatic ?? false;
            set => IsStatic = value;
        }

        bool ICodeElement.IsVirtual
        {
            get => IsVirtual ?? false;
            set => IsVirtual = value;
        }

        bool ICodeElement.IsUnsafe
        {
            get => IsUnsafe ?? false;
            set => IsUnsafe = value;
        }

        MarkerKind ICodeElement.Kind
        {
            get => Kind ?? 0;
            set => Kind = value;
        }

        AccessModifiers ICodeElement.AccessModifiers
        {
            get => AccessModifiers ?? 0;
            set => AccessModifiers = value;
        }

        object ICloneable.Clone()
        {
            return Clone<CodeFilterOptions>();
        }

        /// <summary>
        /// Make a deep copy of the current <see cref="CodeFilterOptions"/> object.
        /// </summary>
        /// <param name="falseIsNull">If true, false values are treated as null for nullable booleans.</param>
        /// <returns>A new copy of the <see cref="CodeFilterOptions"/> object.</returns>
        public CodeFilterOptions Clone(bool falseIsNull = false)
        {
            return new CodeFilterOptions(this, falseIsNull);
        }

        public T Clone<T>() where T : ICodeElement, new()
        {
            var tout = new T();

            ObjectMerge.MergeObjects(this, tout);

            if (ImportInfo != null)
            {
                tout.ImportInfo = (IImportInfo)ImportInfo.Clone();
            }

            if (Attributes != null)
            {
                tout.Attributes = new List<string>(Attributes);
            }

            if (MethodParams != null)
            {
                tout.MethodParams = new List<string>(MethodParams);
            }

            if (Inheritances != null)
            {
                tout.Inheritances = new List<string>(Inheritances);
            }

            return tout;
        }

        /// <summary>
        /// Instantiate a new <see cref="CodeFilterOptions"/> from the specified <see cref="ICodeElement"/> <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="ICodeElement"/> source object.</param>
        /// <param name="falseIsNull">True to treat false values as null for nullable boolean properties.</param>
        public CodeFilterOptions(ICodeElement source, bool falseIsNull = false)
        {
            if (source is CodeFilterOptions cfo)
            {
                ObjectMerge.MergeObjects(cfo, this);
            }
            else
            {
                ObjectMerge.MergeObjects(source, this);
            }

            if (source.ImportInfo != null)
            {
                ImportInfo = (IImportInfo)source.ImportInfo.Clone();
            }

            if (Attributes != null)
            {
                Attributes = new List<string>(Attributes);
            }

            if (MethodParams != null)
            {
                MethodParams = new List<string>(MethodParams);
            }

            if (Inheritances != null)
            {
                Inheritances = new List<string>(Inheritances);
            }

            if (falseIsNull)
            {
                var props1 = codeElementProps;
                var props2 = codeFilterProps;

                foreach (var prop in props1)
                {
                    if (prop.PropertyType == typeof(bool))
                    {
                        var obj = (bool)prop.GetValue(source);
                        if (!obj)
                        {
                            var prop2 = props2.Where(x => x.Name == prop.Name).FirstOrDefault();
                            prop2.SetValue(this, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a new <see cref="CodeFilterOptions"/> options object.
        /// </summary>
        public CodeFilterOptions()
        {
        }

        /// <summary>
        /// Validate an <see cref="ICodeElement"/> object against the current options.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>True if all non-null values are equal for the purposes of this comparison.</returns>
        public virtual bool Validate(ICodeElement element)
        {
            foreach (var prop in codeFilterProps)
            {
                object obj = prop.GetValue(this);

                if (obj == null) continue;

                var prop2 = codeElementProps.Where(x => x.Name == prop.Name).FirstOrDefault();
                object obj2 = prop2?.GetValue(element);

                if (obj2 == null) continue;

                if (obj is bool b1 && obj2 is bool b2)
                {
                    if (b1 != b2) return false;
                }
                else if (obj is string s1 && obj2 is string s2)
                {
                    if (s1 != s2) return false;
                }
                else if (obj is MarkerKind mk1 && obj2 is MarkerKind mk2)
                {
                    if (mk1 != mk2) return false;
                }
                else if (obj is AccessModifiers am1 && obj2 is AccessModifiers am2)
                {
                    if (am1 != am2) return false;
                }
                else if (obj is List<string> ls1 && obj2 is List<string> ls2)
                {
                    if (ls1.Count != ls2.Count) return false;
                    var ls3 = ls1.Intersect(ls2).ToList();
                    if (ls3.Count != ls2.Count) return false;
                }
                else if (obj is IImportInfo ii1 && obj2 is IImportInfo ii2)
                {
                    if (!ii1.Equals(ii2)) return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return $"[CodeFilterOptions] {AccessModifiers} {Kind} {Name}{Generics}".Trim();
        }
    }
}
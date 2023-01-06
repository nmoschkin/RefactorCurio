using DataTools.Code.Project;
using DataTools.Essentials.Helpers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTools.Code.Markers
{
    /// <summary>
    /// Abstract base that implements <see cref="IMarker"/>.
    /// </summary>
    /// <typeparam name="TMarker">The type of <see cref="IMarker"/> that will be used.</typeparam>
    /// <typeparam name="TList">The type of list that will contain child markers.</typeparam>
    internal abstract class MarkerBase<TMarker, TList> : MarkerFinderBase, IMarker<TMarker, TList>
        where TMarker : IMarker, new()
        where TList : IMarkerList<TMarker>, new()
    {
        protected WeakReference<IProjectNode> homeFile;
        protected WeakReference<AtomicGenerationInfo<TMarker, TList>> atomicFile;
        protected WeakReference<IMarker> parentElement;
        protected MarkerKind kind;

        protected TList markers = new TList();
        protected string mpstr;
        protected string name;
        protected string scanHit;
        protected List<string> unknownWords;
        protected string parentElementString;
        protected string content = null;

        public virtual AccessModifiers AccessModifiers { get; set; }

        public virtual string AccessModifierString
        {
            get
            {
                var amstr = new StringBuilder();

                if (AccessModifiers != AccessModifiers.None)
                {
                    var t = AccessModifiers.ToString().ToLower().Split(',');

                    if (t != null && t.Length > 0)
                    {
                        amstr.Append(string.Join(" ", t) + " ");
                    }
                }

                if (IsStatic) amstr.Append("static ");
                if (IsImplicit) amstr.Append("implicit ");
                if (IsExplicit) amstr.Append("explicit ");

                if (IsAbstract) amstr.Append("abstract ");
                if (IsExtern) amstr.Append("extern ");
                if (IsSealed) amstr.Append("sealed ");
                if (IsOverride) amstr.Append("override ");
                if (IsVirtual) amstr.Append("virtual ");
                if (IsAsync) amstr.Append("async ");

                if (Kind == MarkerKind.Delegate) amstr.Append("delegate ");
                if (Kind == MarkerKind.Event) amstr.Append("event ");
                if (kind == MarkerKind.Const) amstr.Append("const ");
                if (kind == MarkerKind.Class) amstr.Append("class ");
                if (kind == MarkerKind.Interface) amstr.Append("interface ");
                if (kind == MarkerKind.Struct) amstr.Append("struct ");
                if (kind == MarkerKind.Record) amstr.Append("record ");

                return amstr.ToString();
            }
        }

        public virtual AtomicGenerationInfo<TMarker, TList> AtomicSourceFile
        {
            get
            {
                if (atomicFile != null && atomicFile.TryGetTarget(out AtomicGenerationInfo<TMarker, TList> target))
                {
                    return target;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                atomicFile = new WeakReference<AtomicGenerationInfo<TMarker, TList>>(value);
            }
        }

        public virtual List<string> Attributes { get; set; }

        /// <summary>
        /// The child elements.
        /// </summary>
        public virtual TList Children
        {
            get => markers;
            set => markers = value;
        }

        IEnumerable IProjectNode.Children => markers;

        IMarkerList IMarker.Children => markers;

        public virtual ElementType ChildType => ElementType.Marker;

        public virtual string Content
        {
            get
            {
                if (content == null)
                {
                    var hf = HomeFile;
                    if (hf is IProjectFile cf)
                    {
                        content = cf.Text?.Substring(StartPos, EndPos - StartPos + 1);
                    }
                }

                return content;
            }
            set
            {
                content = value;
            }
        }

        public virtual string DataType { get; set; }

        public virtual string DataTypeString
        {
            get
            {
                if (!string.IsNullOrEmpty(DataType))
                {
                    if (kind == MarkerKind.Operator)
                    {
                        return DataType + " operator ";
                    }
                    else
                    {
                        return DataType + " ";
                    }
                }

                return "";
            }
        }

        public virtual ElementType ElementType => ElementType.Marker;

        public virtual int EndColumn { get; set; }

        public virtual int EndLine { get; set; }

        public virtual int EndPos { get; set; }

        public virtual string FullyQualifiedName
        {
            get
            {
                if (!string.IsNullOrEmpty(ParentElementPath))
                {
                    return $"{Namespace}.{ParentElementPath}.{Name}";
                }
                else
                {
                    return $"{Namespace}.{Name}";
                }
            }
        }

        public virtual string Generics { get; set; }

        public virtual IProjectNode HomeFile
        {
            get
            {
                if (homeFile != null && homeFile.TryGetTarget(out IProjectNode target))
                {
                    return target;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                homeFile = new WeakReference<IProjectNode>(value);
            }
        }

        public virtual IImportInfo ImportInfo { get; set; }

        public virtual List<string> Inheritances { get; set; }

        public virtual string InheritanceString { get; set; }

        public virtual bool IsAbstract { get; set; }

        public virtual bool IsAsync { get; set; }

        public virtual bool IsExplicit { get; set; }

        public virtual bool IsExtern { get; set; }

        public virtual bool IsImplicit { get; set; }

        public virtual bool IsNew { get; set; }

        public virtual bool IsOverride { get; set; }

        public virtual bool IsReadOnly { get; set; }

        public virtual bool IsSealed { get; set; }

        public virtual bool IsStatic { get; set; }

        public virtual bool IsVirtual { get; set; }

        public virtual MarkerKind Kind
        {
            get => kind;
            set
            {
                if (kind != value)
                {
                    kind = value;
                }
            }
        }

        public virtual int Level { get; set; }

        public virtual List<string> MethodParams { get; set; }

        public virtual string MethodParamsString
        {
            get => mpstr;
            set
            {
                mpstr = value?.Trim() ?? "";

                if (string.IsNullOrEmpty(mpstr))
                {
                    mpstr = "()";
                }
                else if (!(mpstr[0] == '(' && mpstr[mpstr.Length - 1] == ')'))
                {
                    mpstr = "(" + mpstr + ")";
                }
            }
        }

        public virtual string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                }
            }
        }

        public virtual string NameWithGenerics
        {
            get
            {
                if (!string.IsNullOrEmpty(Generics))
                {
                    return $"{FullyQualifiedName}{Generics}";
                }
                else
                {
                    return FullyQualifiedName;
                }
            }
        }

        public virtual string Namespace { get; set; }

        public virtual IMarker ParentElement
        {
            get
            {
                if (parentElement == null || !parentElement.TryGetTarget(out var target))
                {
                    return null;
                }

                return target;
            }
            set
            {
                parentElement = new WeakReference<IMarker>(value);
            }
        }

        ISolutionElement IProjectElement.ParentElement => ParentElement ?? HomeFile;

        public virtual string ParentElementPath
        {
            get => parentElementString;
            set => parentElementString = value;
        }

        public virtual string ScanHit
        {
            get => scanHit;
            set
            {
                if (scanHit != value)
                {
                    scanHit = value;
                    if (scanHit.StartsWith("=")) kind = MarkerKind.Code;
                }
            }
        }

        public virtual int StartColumn { get; set; }

        public virtual int StartLine { get; set; }

        public virtual int StartPos { get; set; }

        public virtual string Title
        {
            get
            {
                return ToString();
            }
        }

        public virtual List<string> UnknownWords
        {
            get => unknownWords;
            set => unknownWords = value;
        }

        public virtual string WhereClause { get; set; }

        /// <summary>
        /// Create a shallow copy of this marker as a <see cref="MarkerBase{TMarker, TList}"/> instance.
        /// </summary>
        /// <returns>A new object of the same type with the same values.</returns>
        public MarkerBase<TMarker, TList> Clone()
        {
            var newItem = (MarkerBase<TMarker, TList>)MemberwiseClone();
            return newItem;
        }

        T IMarker.Clone<T>(bool deep)
        {
            var pis = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            var r = new T();

            if (r is MarkerBase<TMarker, TList> newItem)
            {
                // the ideal situation!  a common ancester (this class)
                ObjectMerge.MergeObjects(this, newItem);

                if (!deep) return r;

                newItem.Children = new TList();

                foreach (var item in markers)
                {
                    newItem.Children.Add(item.Clone<TMarker>(true));
                }

                return r;
            }

            // the less ideal situation...

            // This is a pretty open interface with many setters, and since we can't copy the object as a descendant
            // because we don't know from whence the object comes, we can invoke its interface members, instead.
            foreach (var pi in pis)
            {
                object gp = null;

                // try to get the interface property.
                var iis = typeof(IMarker).GetProperty(pi.Name);

                if (iis != null && iis.CanWrite)
                {
                    gp = iis.GetValue(this, null);
                }
                else
                {
                    // we can try to see if this works.

                    // the only reason to exhaustively check for common ancestry
                    // is to throw an argument exception, and that will happen, regardless.

                    if (!pi.CanWrite) continue;

                    // if it breaks, you'll know not to do that again.
                    gp = pi.GetValue(this);
                }

                if (gp != null)
                {
                    // create a list the hard way
                    if (deep && pi.Name == nameof(IMarker.Children) && gp is IMarkerList gpList)
                    {
                        if (!(pi.GetValue(r) is object))
                        {
                            try
                            {
                                pi.SetValue(r, Activator.CreateInstance(pi.PropertyType));
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        if (pi.GetValue(r) is IList ml)
                        {
                            foreach (var item in markers)
                            {
                                ml.Add(item.Clone<T>(true));
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        pi.SetValue(r, gp);
                    }
                }
            }

            return r;
        }

        /// <summary>
        /// Clone this marker into another object derived from <see cref="MarkerBase{TMarker, TList}"/>.
        /// </summary>
        /// <param name="deep">Deeply copy the item by making a new collection for and copies of the children.</param>
        /// <typeparam name="T">The type of object to create, must be creatable.</typeparam>
        /// <returns>A new object based on this one.</returns>
        public T Clone<T>(bool deep) where T : MarkerBase<TMarker, TList>, new()
        {
            var newItem = new T();
            try
            {
                ObjectMerge.MergeObjects(this, newItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (!deep) return newItem;

            newItem.Children = new TList();

            foreach (var item in markers)
            {
                newItem.Children.Add(item.Clone<TMarker>(true));
            }

            return newItem;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Find the first ancestor with the specified kind.
        /// </summary>
        /// <param name="parentKind"></param>
        /// <returns></returns>
        public abstract TMarker FindParent(MarkerKind parentKind);

        public override string ToString()
        {
            if (StartLine == EndLine)
            {
                return $"{Kind} {Name}{Generics}, Line {StartLine}";
            }
            else
            {
                return $"{Kind} {Name}{Generics}, Line {StartLine} to {EndLine}";
            }
        }

        /// <summary>
        /// Gets the formatted contents of the entire marker.
        /// </summary>
        /// <returns></returns>
        public abstract string FormatContents();

        public override IMarker GetMarkerAtLine(int line)
        {
            foreach (var marker in Children)
            {
                var chm = ScanMarker(marker, (m) =>
                {
                    return line >= m.StartLine && line <= m.EndLine;
                });

                if (chm != null) return chm;
            }

            return null;
        }

        public override IMarker GetMarkerAt(int index)
        {
            foreach (var marker in Children)
            {
                var chm = ScanMarker(marker, (m) =>
                {
                    return index >= m.StartPos && index <= m.EndPos;
                });

                if (chm != null) return chm;
            }

            return null;
        }
    }
}
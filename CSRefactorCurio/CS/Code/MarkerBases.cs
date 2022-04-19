using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTools.MathTools;
using System.Diagnostics.Contracts;
using DataTools.SortedLists;
using DataTools.Observable;
using Microsoft.Build.Framework.XamlTypes;
using System.Text.RegularExpressions;
using MessagePack;

namespace DataTools.CSTools
{


    /// <summary>
    /// Represents a rendered and reformatted source code file containing the preamble and only the specified items from the original code.
    /// </summary>
    /// <typeparam name="TElem">The type of <see cref="IMarker"/> that will be used.</typeparam>
    /// <typeparam name="TList">The type of list that will contain the markers.</typeparam>
    public class RenderedFile<TElem, TList> where TElem : IMarker, new() where TList : IList<TElem>, new()
    {
        /// <summary>
        /// Character position in the source file where the preamble begins.
        /// </summary>
        public int PreambleBegin { get; set; }

        /// <summary>
        /// Character position in the source file where the preamble ends.
        /// </summary>
        public int PreambleEnd { get; set; }

        /// <summary>
        /// The <see cref="IMarker"/> collection.
        /// </summary>
        public TList Markers { get; set; }

        /// <summary>
        /// The source code document lines.
        /// </summary>
        public IList<string> Lines { get; set; }

    }


    /// <summary>
    /// Detected access modifiers
    /// </summary>
    [Flags]
    public enum AccessModifiers
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Private
        /// </summary>
        Private = 0x1,

        /// <summary>
        /// Protected
        /// </summary>
        Protected = 0x2,

        /// <summary>
        /// Internal
        /// </summary>
        Internal = 0x4,

        /// <summary>
        /// public
        /// </summary>
        Public = 0x8,
    }

    /// <summary>
    /// Represents a logical segment in a source code file.
    /// </summary>
    public interface IMarker : IProjectNode, ICloneable
    {
        /// <summary>
        /// If applicable, the <see cref="AccessModifiers"/> for the element.
        /// </summary>
        AccessModifiers AccessModifiers { get; set; }

        /// <summary>
        /// Element is marked virtual.
        /// </summary>
        bool IsVirtual { get; set; }

        /// <summary>
        /// Element is marked abstract.
        /// </summary>
        bool IsAbstract { get; set; }


        /// <summary>
        /// Element is marked extern.
        /// </summary>
        bool IsExtern { get; set; }

        /// <summary>
        /// Element is marked readonly.
        /// </summary>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// Element is marked override.
        /// </summary>
        bool IsOverride { get; set; }

        /// <summary>
        /// Element is marked new.
        /// </summary>
        bool IsNew { get; set; }

        /// <summary>
        /// Element is a static member.
        /// </summary>
        bool IsStatic { get; set; }

        /// <summary>
        /// Element is an async method.
        /// </summary>
        bool IsAsync { get; set; }

        /// <summary>
        /// Home namespace of this element.
        /// </summary>
        string Namespace { get; set; }

        /// <summary>
        /// The string content of this element block.
        /// </summary>
        string Content { get; set; }

        /// <summary>
        /// The scanned text that was used to determine the nature of the current element.
        /// </summary>
        string ScanHit { get; set; }

        /// <summary>
        /// If applicable, the method parameter string.
        /// </summary>
        string MethodParamsString { get; set; }

        /// <summary>
        /// If applicable, the list of method parameters.
        /// </summary>
        List<string> MethodParams { get; set; }


        /// <summary>
        /// If applicable, the list of attributes.
        /// </summary>
        List<string> Attributes { get; set; }

        /// <summary>
        /// The first character position in the original document.
        /// </summary>
        int StartPos { get; set; }

        /// <summary>
        /// The last character position in the original document.
        /// </summary>
        int EndPos { get; set; }

        /// <summary>
        /// The start line in the original document.
        /// </summary>
        int StartLine { get; set; }

        /// <summary>
        /// The end line in the original document.
        /// </summary>
        int EndLine { get; set; }

        /// <summary>
        /// The start column in the original document.
        /// </summary>
        int StartColumn { get; set; }

        /// <summary>
        /// The end column in the original document.
        /// </summary>
        int EndColumn { get; set; }

        /// <summary>
        /// The identified element marker kind.
        /// </summary>
        MarkerKind Kind { get; set; }

        /// <summary>
        /// The name of the element.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// If applicable, gets the data type of the element.
        /// </summary>
        string DataType { get; set; }

        /// <summary>
        /// If applicable, the generic type parameters of this element.
        /// </summary>
        string Generics { get; set; }

        /// <summary>
        /// If applicable, the inheritence of this element.
        /// </summary>
        string Inheritance { get; set; }

        /// <summary>
        /// If applicable, the where clause of this element.
        /// </summary>
        string WhereClause { get; set; }


        /// <summary>
        /// The logical hierarchical level of this element in the original document.
        /// </summary>
        int Level { get; set; }

        /// <summary>
        /// The child elements.
        /// </summary>
        new IMarkerList Children { get; }

        /// <summary>
        /// Clone this marker into another object that implmenets <see cref="IMarker"/>.
        /// </summary>
        /// <param name="deep">Deeply copy the item by making a new collection for and copies of the children.</param>
        /// <typeparam name="T">The type of object to create, must be creatable.</typeparam>
        /// <returns>A new object based on this one.</returns>
        /// <remarks>Implementations should make note of when and where they cannot fulfill the <paramref name="deep"/> contract.</remarks>
        T Clone<T>(bool deep) where T : IMarker, new();
   
    }

    /// <summary>
    /// Abstract base that imlements <see cref="IMarker"/>.
    /// </summary>
    /// <typeparam name="TElem">The type of <see cref="IMarker"/> that will be used.</typeparam>
    /// <typeparam name="TList">The type of list that will contain child markers.</typeparam>
    public abstract class MarkerBase<TElem, TList> : IMarker<TElem, TList> 
        where TElem : IMarker, new() 
        where TList : IMarkerList<TElem>, new()
    {
        protected TList markers = new TList();
        protected string scanHit;
        protected string name;
        protected MarkerKind kind;
        protected string mpstr;

        /// <summary>
        /// Create a shallow copy of this marker as a <see cref="MarkerBase{TElem, TList}"/> instance.
        /// </summary>
        /// <returns>A new object of the same type with the same values.</returns>
        public MarkerBase<TElem, TList> Clone()
        {
            var newItem = (MarkerBase<TElem, TList>)MemberwiseClone();
            return newItem;
        }

        T IMarker.Clone<T>(bool deep)
        {

            var pis = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            var r = new T();

            if (r is MarkerBase<TElem, TList> newItem)
            {
                // the ideal situation!  a common ancester (this class)
                ObjectMerge.MergeObjects(this, newItem);

                if (!deep) return r;

                newItem.Children = new TList();

                foreach (var item in markers)
                {
                    newItem.Children.Add(item.Clone<TElem>(true));
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
                    gp  = iis.GetValue(this, null);
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
                    if (deep && (pi.Name == nameof(IMarker.Children) && gp is IMarkerList gpList))
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
        /// Clone this marker into another object derived from <see cref="MarkerBase{TElem, TList}"/>.
        /// </summary>
        /// <param name="deep">Deeply copy the item by making a new collection for and copies of the children.</param>
        /// <typeparam name="T">The type of object to create, must be creatable.</typeparam>
        /// <returns>A new object based on this one.</returns>
        public T Clone<T>(bool deep) where T : MarkerBase<TElem, TList>, new()
        {
            var newItem = new T();

            ObjectMerge.MergeObjects(this, newItem);

            if (!deep) return newItem;

            newItem.Children = new TList();

            foreach (var item in markers)
            {
                newItem.Children.Add(item.Clone<TElem>(true));
            }

            return newItem;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        private void ScanForDataType()
        {
            return;

            //if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(ScanHit)) return;

            //switch (kind)
            //{
            //    case MarkerKind.Method:
            //    case MarkerKind.Property:
            //    case MarkerKind.Field:
            //    case MarkerKind.Event:
            //    case MarkerKind.Const:
            //    case MarkerKind.Delegate:
            //        break;

            //    default:
            //        return;
            //}

            //var deletes = new string[] { "public", "private", "static", "async", "abstract", "explicit", "implicit", "const", "readonly", "unsafe", "fixed", "delegate", "event", "virtual", "protected", "internal", "override", "new" };
            //var re = new Regex(@"\[(.+)\]");

            //var sh = ScanHit;

            //sh = re.Replace(sh, "");
            //re = new Regex(@"\=.+");
            //sh = re.Replace(sh, "");

            //foreach (var d in deletes) sh = sh.Replace(d, "");
            //sh = sh.Trim();

            //re = new Regex(@"^(.+)\s+" + Name);

            //var m = re.Match(sh);
            //if (m.Success)
            //{
            //    DataType = m.Groups[1].Value;
            //}
        }

        public virtual AccessModifiers AccessModifiers { get; set; }

        public virtual string AccessModifierString
        {
            get
            {
                var amstr = "";

                if (AccessModifiers != AccessModifiers.None) 
                {
                    var t = AccessModifiers.ToString().ToLower().Split(',');
                    amstr = string.Join(" ", t);
                    if (amstr != "") amstr += " ";
                }

                if (IsStatic) amstr += "static ";

                if (IsAbstract) amstr += "abstract ";
                if (IsOverride) amstr += "override ";
                if (IsVirtual) amstr += "virtual ";
                if (IsAsync) amstr += "async ";

                if (Kind == MarkerKind.Delegate) amstr += "delegate ";
                if (Kind == MarkerKind.Event) amstr += "event ";
                if (kind == MarkerKind.Const) amstr += "const ";
                if (kind == MarkerKind.Class) amstr += "class ";
                if (kind == MarkerKind.Interface) amstr += "interface ";
                if (kind == MarkerKind.Struct) amstr += "struct ";
                if (kind == MarkerKind.Record) amstr += "record ";
                
                return amstr;
            }
        }

        public virtual string Inheritance { get; set; }

        public virtual string WhereClause { get; set; }
        public virtual bool IsVirtual { get; set; }

        public virtual bool IsAbstract { get; set; }

        public virtual bool IsExtern { get; set; }

        public virtual bool IsReadOnly { get; set; }

        public virtual bool IsOverride { get; set; }

        public virtual bool IsNew { get; set; }

        public virtual bool IsStatic { get; set; }

        public virtual bool IsAsync { get; set; }

        public virtual string Namespace { get; set; }

        public virtual string Content { get; set; }

        public virtual string ScanHit
        {
            get => scanHit;
            set
            {
                if (scanHit != value)
                {
                    scanHit = value;
                    if (scanHit.StartsWith("=")) kind = MarkerKind.Code;
                    if (scanHit != null && name != null)
                    {
                        ScanForDataType();
                    }
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
                    
                    if (scanHit != null && name != null)
                    {
                        ScanForDataType();
                    }
                }
            }
        }

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

        public virtual List<string> MethodParams { get; set; }

        public virtual int StartPos { get; set; }

        public virtual int EndPos { get; set; }

        public virtual int StartLine { get; set; }

        public virtual int EndLine { get; set; }

        public virtual int StartColumn { get; set; }

        public virtual int EndColumn { get; set; }

        public virtual MarkerKind Kind
        {
            get => kind;
            set
            {
                if (kind != value)
                {
                    kind = value;

                    if (scanHit != null && name != null)
                    {
                        ScanForDataType();
                    }

                }
            }
        }

        public virtual string DataType { get; set; }

        public virtual string DataTypeString
        {
            get
            {
                if (!string.IsNullOrEmpty(DataType)) return DataType + " ";
                return "";
            }
        }

        public virtual string Generics { get; set; }

        public virtual int Level { get; set; }

        public virtual string Title
        {
            get
            {
                return ToString();
            }
        }

        public virtual ElementType ElementType => ElementType.Marker;
        public virtual ElementType ChildType => ElementType.Marker;
        public virtual List<string> Attributes { get; set; }

        public virtual RenderedFile<TElem, TList> ParentFile { get; set; }

        /// <summary>
        /// The child elements.
        /// </summary>
        public virtual TList Children
        {
            get => markers;
            set => markers = value;
        }

        IList IProjectNode.Children => (IList)markers;

        IMarkerList IMarker.Children => markers;

        public override string ToString()
        {
            return $"{Kind} {Name}{Generics}, Line: {StartLine} to {EndLine}";
        }

    }

    /// <summary>
    /// Represents a strongly typed <see cref="IMarker"/> implementation with a strongly typed <see cref="IMarkerList{TElem}"/> implementation for storing child markers.
    /// </summary>
    /// <typeparam name="TElem"></typeparam>
    /// <typeparam name="TList"></typeparam>
    public interface IMarker<TElem, TList> : IMarker where TElem : IMarker, new() where TList : IMarkerList<TElem>, new()
    {
        /// <summary>
        /// The parent source file information object.
        /// </summary>
        RenderedFile<TElem, TList> ParentFile { get; set; }

        /// <summary>
        /// The child elements.
        /// </summary>
        new TList Children { get; set; }

    }

    /// <summary>
    /// Represents the base for marker filter rules.
    /// </summary>
    public abstract class MarkerFilterRule
    {
        /// <summary>
        /// Determines whether the specified item is valid for the filter rule.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>True if the item passes the filter, otherwise false.</returns>
        public abstract bool IsValid(IMarker item);

    }

    /// <summary>
    /// Base class for a strongly-typed, self-applying filter rule.
    /// </summary>
    /// <typeparam name="TElem">The <see cref="IMarker"/> element type.</typeparam>
    /// <typeparam name="TList">The <see cref="IMarkerList{TElem}"/> type.</typeparam>
    public abstract class MarkerFilterRule<TElem, TList> : MarkerFilterRule where TElem: IMarker, new() where TList: IMarkerList<TElem>, new()  
    {
        /// <summary>
        /// Filter the items based on the rule rule.
        /// </summary>
        /// <param name="items">The items to filter.</param>
        /// <returns>A filtered list of items.</returns>
        public abstract TList ApplyFilter(TList items);
    }


    /// <summary>
    /// Base class for a strongly-typed, self-applying sort filter.
    /// </summary>
    /// <typeparam name="TElem">The <see cref="IMarker"/> element type.</typeparam>
    /// <typeparam name="TList">The <see cref="IMarkerList{TElem}"/> type.</typeparam>
    public abstract class SortFilterRule<TElem, TList> : MarkerFilterRule<TElem, TList>, IComparer<TElem> where TElem: IMarker, new() where TList: IMarkerList<TElem>, new()
    {
        public abstract int Compare(TElem x, TElem y);

        public override TList ApplyFilter(TList items)
        {
            var newItems = new TList();
            foreach (var item in items)
            {
                if (IsValid(item))
                {
                    newItems.Add(item);
                }
            }

            QuickSort.Sort(newItems, Compare);
            return newItems;
        }
    }

    /// <summary>
    /// A simple single positive identity filter rule.
    /// </summary>
    public class MarkerKindFilterRule : MarkerFilterRule
    {
        public MarkerKind Kind { get; }

        public override bool IsValid(IMarker item)
        {
            return Kind == item.Kind;
        }

        public MarkerKindFilterRule(MarkerKind kind)
        {
            Kind = kind;
        }
    }

    /// <summary>
    /// A simple single positive identity filter rule based on level and kind.
    /// </summary>
    public class MarkerLevelFilterRule : MarkerKindFilterRule
    {
        /// <summary>
        /// Gets the allowed level for this rule.
        /// </summary>
        public int Level { get; }

        public override bool IsValid(IMarker item)
        {
            return Kind == item.Kind && Level == item.Level; 
        }

        /// <summary>
        /// Create a new market filter level rule.
        /// </summary>
        /// <param name="kind">The kind</param>
        /// <param name="level">The level</param>
        public MarkerLevelFilterRule(MarkerKind kind, int level) : base(kind)
        {
            Level = level;
        }

    }

    /// <summary>
    /// Filter Chain Kinds
    /// </summary>
    public enum FilterChainKind
    {
        /// <summary>
        /// All rules must pass for validity.
        /// </summary>
        PassAll,

        /// <summary>
        /// Any rule must pass for validity.
        /// </summary>
        PassAny,
    }

    /// <summary>
    /// A simple and open chained filter rule.
    /// </summary>
    public class MarkerFilterRuleChain : MarkerFilterRule
    {
        private List<MarkerFilterRule> rules;

        /// <summary>
        /// Gets or sets the rule chain that will be used to validate items.
        /// </summary>
        public virtual List<MarkerFilterRule> RuleChain
        {
            get => rules;
            set => rules = value;
        }

        /// <summary>
        /// Gets or sets the kind of chain (pass all or pass any).
        /// </summary>
        public virtual FilterChainKind FilterChainKind { get; set; } = FilterChainKind.PassAll;

        /// <summary>
        /// Create a new marker rule chain.
        /// </summary>
        public MarkerFilterRuleChain()
        {
            rules = new List<MarkerFilterRule>();
        }

        /// <summary>
        /// Create a new marker rule chain from the specified initial starting values.
        /// </summary>
        /// <param name="rules">The initial starting values.</param>
        public MarkerFilterRuleChain(IEnumerable<MarkerFilterRule> rules)
        {
            this.rules = new List<MarkerFilterRule>(rules);
        }

        /// <summary>
        /// Runs each element in the rule chain, in enumeration order.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>True if the item passes, otherwise false.</returns>
        public override bool IsValid(IMarker item)
        {
            if (FilterChainKind == FilterChainKind.PassAll)
            {
                foreach (var rule in rules)
                {
                    if (!rule.IsValid(item)) return false;
                }

                return true;
            }
            else if (FilterChainKind == FilterChainKind.PassAny)
            {
                foreach (var rule in rules)
                {
                    if (rule.IsValid(item)) return true;
                }

                return false;
            }

            return false;
        }
    }

    /// <summary>
    /// A simple and open chained filter rule for strongly typed filters.
    /// </summary>
    public class MarkerFilterRuleChain<TElem, TList> : MarkerFilterRule<TElem, TList>
        where TList : IMarkerList<TElem>, new()
        where TElem : IMarker<TElem, TList>, new()
    {
        private List<MarkerFilterRule<TElem, TList>> rules;

        /// <summary>
        /// Gets or sets the rule chain that will be used to validate items.
        /// </summary>
        public virtual List<MarkerFilterRule<TElem, TList>> RuleChain
        {
            get => rules;
            set => rules = value;
        }

        /// <summary>
        /// Gets or sets the kind of chain (pass all or pass any).
        /// </summary>
        public virtual FilterChainKind FilterChainKind { get; set; } = FilterChainKind.PassAll;

        /// <summary>
        /// Create a new marker rule chain.
        /// </summary>
        public MarkerFilterRuleChain()
        {
            rules = new List<MarkerFilterRule<TElem, TList>>();
        }

        /// <summary>
        /// Create a new marker rule chain from the specified initial starting values.
        /// </summary>
        /// <param name="rules">The initial starting values.</param>
        public MarkerFilterRuleChain(IEnumerable<MarkerFilterRule<TElem, TList>> rules)
        {
            this.rules = new List<MarkerFilterRule<TElem, TList>>(rules);
        }

        /// <summary>
        /// Runs each element in the rule chain, in enumeration order.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>True if the item passes, otherwise false.</returns>
        public override bool IsValid(IMarker item)
        {
            if (FilterChainKind == FilterChainKind.PassAll)
            {
                foreach (var rule in rules)
                {
                    if (!rule.IsValid(item)) return false;
                }

                return true;
            }
            else if (FilterChainKind == FilterChainKind.PassAny)
            {
                foreach (var rule in rules)
                {
                    if (rule.IsValid(item)) return true;
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// Runs each filter in succession, using the results of the previous filter in the chain to run the next filter in the chain.
        /// </summary>
        /// <param name="items">The final list of items.</param>
        /// <returns></returns>
        public override TList ApplyFilter(TList items)
        {
            TList result = items;

            foreach (var rule in rules)
            {
                result = rule.ApplyFilter(result);
            }

            return result;
        }
    }


    /// <summary>
    /// A simple and open chained filter rule for strongly typed filters.
    /// </summary>
    public abstract class FixedFilterRuleChain<TElem, TList> : MarkerFilterRule<TElem, TList>
        where TList : IMarkerList<TElem>, new()
        where TElem : IMarker<TElem, TList>, new()
    {
        MarkerFilterRuleChain<TElem, TList> filterChain;

        /// <summary>
        /// Gets the kind of filter chain (validate any or validate all).
        /// </summary>
        public abstract FilterChainKind FilterChainKind { get; }

        /// <summary>
        /// Provide the filter chain.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<MarkerFilterRule<TElem, TList>> ProvideFilterChain();
        
        /// <summary>
        /// Create a new fixed filter rule chain.
        /// </summary>
        public FixedFilterRuleChain()
        {
            filterChain = new MarkerFilterRuleChain<TElem, TList>();
            filterChain.FilterChainKind = FilterChainKind;

            var newRules = ProvideFilterChain();

            foreach (var rule in newRules)
            {
                filterChain.RuleChain.Add(rule);
            }
        }

        /// <summary>
        /// Runs each element in the rule chain, in enumeration order.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>True if the item passes, otherwise false.</returns>
        public override bool IsValid(IMarker item)
        {
            return filterChain.IsValid(item);
        }

        /// <summary>
        /// Runs each filter in succession, using the results of the previous filter in the chain to run the next filter in the chain.
        /// </summary>
        /// <param name="items">The final list of items.</param>
        /// <returns></returns>
        public override TList ApplyFilter(TList items)
        {
            return filterChain.ApplyFilter(items);
        }

    }


    /// <summary>
    /// Interface for an object that works with filter rules to filter <see cref="IMarker"/> items.
    /// </summary>
    /// <typeparam name="TElem">The <see cref="IMarker"/> to filter.</typeparam>
    /// <typeparam name="TList">The <see cref="IMarker{TElem, TList}"/>.</typeparam>
    public class MarkerFilter<TElem, TList> 
        where TList : IMarkerList<TElem>, new() 
        where TElem : IMarker<TElem, TList>, new()
    {
        /// <summary>
        /// Apply the given rule, return a new list of items based on the specified rule.
        /// </summary>
        /// <param name="items">The items to filter.</param>
        /// <param name="rule">The rule to apply.</param>
        /// <param name="recursiveForSimpleRules">True to run rules based on <see cref="MarkerFilterRule"/> recursively.</param>
        /// <returns>The filtered list.</returns>
        /// <remarks>
        /// Simple rules are run recursively as specified by the <paramref name="recursiveForSimpleRules"/> parameter.<br /><br />
        /// Rules based on <see cref="MarkerFilterRule{TElem, TList}"/> must define their own recursion tactics.
        /// </remarks>
        public virtual TList ApplyFilter(TList items, MarkerFilterRule rule, bool recursiveForSimpleRules = true)
        {
            // strong rules have their own filtering.
            if (rule is MarkerFilterRule<TElem, TList> strongRule)
            {
                return ApplyFilter(items, strongRule);
            }

            var newList = new TList();

            foreach (var item in items)
            {
                if (rule.IsValid(item))
                {
                    // we want a shallow copy, here.  The filter recursion will create a deeper copy for children.
                    var newItem = (TElem)item.Clone();    

                    newList.Add(newItem);
                    ApplyFilter(newItem.Children, rule);
                }
            }

            return newList;
        }

        /// <summary>
        /// Apply the given rule by calling the <see cref="MarkerFilterRule{TElem, TList}.ApplyFilter(TList)"/> method.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public virtual TList ApplyFilter(TList items, MarkerFilterRule<TElem, TList> rule)
        {
            return rule.ApplyFilter(items);
        }
    }

    /// <summary>
    /// A base class for providing a system for filtering <see cref="IMarker"/> items.
    /// </summary>
    /// <typeparam name="TElem">The type of <see cref="IMarker"/> item.</typeparam>
    /// <typeparam name="TList">The type of <see cref="IMarkerList{TElem}"/>.</typeparam>
    /// <typeparam name="TFilter">The type of <see cref="MarkerFilter{TElem, TList}"/>.</typeparam>
    public interface IMarkerFilterProvider<TElem, TList, TFilter> 
        where TList : IMarkerList<TElem>, new()
        where TElem : IMarker<TElem, TList>, new()
        where TFilter : MarkerFilter<TElem, TList>, new()
    {
        
        /// <summary>
        /// Gets the list of items after the filters have been applied.
        /// </summary>
        TList FilteredItems { get; }

        /// <summary>
        /// Gets the filter engine currently being used by the filter provider
        /// </summary>
        TFilter Filter { get; }

        /// <summary>
        /// Provides a filter based on the given element context.
        /// </summary>
        /// <param name="items">The list to provide the filter for.</param>
        /// <returns></returns>
        MarkerFilterRule ProvideFilterRule(TList items);

        /// <summary>
        /// Run the filter on the given list of items.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>A new list of items that have been filtered.</returns>
        TList RunFilters(TList items);

    }

    /// <summary>
    /// A base class for providing a system for filtering <see cref="IMarker"/> items using the default <see cref="MarkerFilter{TElem, TList}"/> engine..
    /// </summary>
    /// <typeparam name="TElem">The type of <see cref="IMarker"/> item.</typeparam>
    /// <typeparam name="TList">The type of <see cref="IMarkerList{TElem}"/>.</typeparam>
    public interface IMarkerFilterProvider<TElem, TList> : IMarkerFilterProvider<TElem, TList, MarkerFilter<TElem, TList>>
        where TList : IMarkerList<TElem>, new()
        where TElem : IMarker<TElem, TList>, new()
    {
    }

}

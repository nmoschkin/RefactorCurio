using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DataTools.Code.Project
{
    /// <summary>
    /// Base class for project nodes with a specific kind of list of <see cref="IProjectElement"/>.
    /// </summary>
    /// <typeparam name="TList"></typeparam>
    internal abstract class ProjectNodeBase<TList> : ProjectNodeBase<TList, IProjectElement>, IProjectNode<TList>
        where TList : IList<IProjectElement>, INotifyCollectionChanged, INotifyPropertyChanged, new()
    {
        /// <summary>
        /// Creates a new project node
        /// </summary>
        /// <param name="parent">Optional parent</param>
        /// <param name="children">Optional children of type <typeparamref name="TItem"/></param>
        public ProjectNodeBase(ISolutionElement parent = null, IEnumerable<IProjectElement> children = null) : base(parent, children)
        {
        }
    }

    /// <summary>
    /// Base class for project nodes with a specific kind of list of a specific kind of child that implements <see cref="IProjectElement"/>.
    /// </summary>
    /// <typeparam name="TList"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    internal abstract class ProjectNodeBase<TList, TItem> : ProjectElementBase, IProjectNode<TList, TItem>
        where TList : IList<TItem>, INotifyCollectionChanged, INotifyPropertyChanged, new()
        where TItem : IProjectElement

    {
        protected TList children = new TList();

        /// <summary>
        /// Creates a new project node
        /// </summary>
        /// <param name="parent">Optional parent</param>
        /// <param name="children">Optional children of type <typeparamref name="TItem"/></param>
        public ProjectNodeBase(ISolutionElement parent = null, IEnumerable<TItem> children = null) : base(parent)
        {
            if (children != null)
            {
                foreach (TItem pobj in children)
                {
                    this.children.Add(pobj);
                }
            }
        }

        public virtual TList Children
        {
            get => children;
            protected set
            {
                SetProperty(ref children, value);
            }
        }

        public abstract ElementType ChildType { get; }

        System.Collections.IEnumerable IProjectNode.Children => children;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DataTools.Code.Project
{
    /// <summary>
    /// Represents a project element with child elements.
    /// </summary>
    internal interface IProjectNode : IProjectElement
    {
        /// <summary>
        /// The element children.
        /// </summary>
        IEnumerable Children { get; }

        /// <summary>
        /// The type flags for the element children (can be or'd)
        /// </summary>
        ElementType ChildType { get; }
    }

    /// <summary>
    /// Represents a project element with child elements that can be observed.
    /// </summary>
    /// <typeparam name="T">The type of collection the project children will consist of</typeparam>
    internal interface IProjectNode<T> : IProjectNode where T : IEnumerable<IProjectElement>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the list of child nodes.
        /// </summary>
        new T Children { get; }
    }

    /// <summary>
    /// Represents a project element with child elements that can be observed.
    /// </summary>
    /// <typeparam name="TList">The type of collection the project children will consist of</typeparam>
    internal interface IProjectNode<TList, TItem> : IProjectNode
        where TList : IList<TItem>, INotifyCollectionChanged, INotifyPropertyChanged
        where TItem : IProjectElement
    {
        /// <summary>
        /// Gets the list of child nodes.
        /// </summary>
        new TList Children { get; }
    }
}
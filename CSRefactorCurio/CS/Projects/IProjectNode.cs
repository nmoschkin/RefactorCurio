using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DataTools.CSTools
{
    /// <summary>
    /// Represents a project element with child elements.
    /// </summary>
    internal interface IProjectNode : IProjectElement
    {
        /// <summary>
        /// The element children.
        /// </summary>
        IList Children { get; }

        /// <summary>
        /// The type flags for the element children (can be or'd)
        /// </summary>
        ElementType ChildType { get; }
    }

    /// <summary>
    /// Represents a project element with child elements that can be observed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IProjectNode<T> : IProjectNode where T : IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the list of child nodes.
        /// </summary>
        new T Children { get; }
    }
}
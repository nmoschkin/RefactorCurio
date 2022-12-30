using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DataTools.CSTools
{
    /// <summary>
    /// Represents a project element with child elements based on a file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IProjectFile : IProjectNode
    {
        string Filename { get; }

        string Text { get; }
    }

    internal interface IProjectFile<T> : IProjectFile, IProjectNode<T> where T : IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }
}
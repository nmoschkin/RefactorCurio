﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DataTools.Code.Project
{
    /// <summary>
    /// Represents a project element with child elements based on a file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProjectFile : IProjectNode
    {
        string Filename { get; }

        string Text { get; }
    }

    public interface IProjectFile<T> : IProjectFile, IProjectNode<T> where T : IEnumerable<IProjectElement>, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }

    public interface IProjectFile<TList, TItem> : IProjectFile, IProjectNode<TList, TItem>
        where TList : IList<TItem>, INotifyCollectionChanged, INotifyPropertyChanged
        where TItem : IProjectElement
    {
    }
}
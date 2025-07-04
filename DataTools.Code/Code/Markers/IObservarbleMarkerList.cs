﻿using System.Collections.Specialized;
using System.ComponentModel;

namespace DataTools.Code.Markers
{
    /// <summary>
    /// Base interface for strongly-typed, observable marker lists.
    /// </summary>
    /// <typeparam name="TMarker">The <see cref="IMarker"/></typeparam>
    public interface IObservarbleMarkerList<TMarker> : IMarkerList<TMarker>, INotifyCollectionChanged, INotifyPropertyChanged where TMarker : IMarker
    {
    }
}
using System.Collections.Specialized;
using System.ComponentModel;

namespace DataTools.CSTools
{
    /// <summary>
    /// Base interface for strongly-typed, observable marker lists.
    /// </summary>
    /// <typeparam name="TMarker">The <see cref="IMarker"/></typeparam>
    internal interface IObservarbleMarkerList<TMarker> : IMarkerList<TMarker>, INotifyCollectionChanged, INotifyPropertyChanged where TMarker : IMarker
    {
    }
}
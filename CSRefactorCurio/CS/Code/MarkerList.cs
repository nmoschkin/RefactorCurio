
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace DataTools.CSTools
{

    /// <summary>
    /// Base interface for marker lists. Inherits from <see cref="IEnumerable"/>.
    /// </summary>
    public interface IMarkerList : IEnumerable
    {
    }

    /// <summary>
    /// Base interface for strongly-typed marker lists.
    /// </summary>
    /// <typeparam name="TElem">The <see cref="IMarker"/></typeparam>
    public interface IMarkerList<TElem> : IMarkerList, IList<TElem> where TElem : IMarker
    {
    }

    /// <summary>
    /// Base interface for strongly-typed, observable marker lists.
    /// </summary>
    /// <typeparam name="TElem">The <see cref="IMarker"/></typeparam>
    public interface IObservarbleMarkerList<TElem> : IMarkerList<TElem>, INotifyCollectionChanged, INotifyPropertyChanged where TElem : IMarker
    {
    }

    /// <summary>
    /// Marker list, standard implementation.
    /// </summary>
    /// <typeparam name="TElem">The <see cref="IMarker"/></typeparam>
    public class MarkerList<TElem> : Collection<TElem>, IMarkerList<TElem> where TElem : IMarker
    {

    }

    /// <summary>
    /// Observable marker list, standard implementation.
    /// </summary>
    /// <typeparam name="TElem">The <see cref="IMarker"/></typeparam>
    public class ObservableMarkerList<TElem> : MarkerList<TElem>, IObservarbleMarkerList<TElem> where TElem : IMarker
    {
        /// <summary>
        /// Gets or sets a value indicating that <see cref="INotifyCollectionChanged"/> events will not be fired.
        /// </summary>
        protected bool DisableEvents { get; set; }

        /// <summary>
        /// Sync Lock Object
        /// </summary>
        protected object SyncRoot { get; } = new object();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        // <summary>
        /// Set a property value if the current value is not equal to the new value and raise the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="backingStore">The value to compare and set.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns></returns>
        protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            bool pass;
            if (typeof(T).IsValueType)
            {
                pass = !backingStore.Equals(value);
            }
            else
            {
                if (!(backingStore is object) && !(value is object))
                {
                    pass = false;
                }
                else if (backingStore is object && !(value is object))
                {
                    pass = true;
                }
                else if (!(backingStore is object) && value is object)
                {
                    pass = true;
                }
                else
                {
                    pass = !backingStore.Equals(value);
                }
            }

            if (pass)
            {
                backingStore = value;
                OnPropertyChanged(propertyName);
            }

            return pass;
        }

        /// <summary>
        /// Raise <see cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raise <see cref="INotifyCollectionChanged.CollectionChanged"/>.
        /// </summary>
        /// <param name="e"></param>
        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!DisableEvents)
            {
                CollectionChanged?.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }

        }

        protected override void InsertItem(int index, TElem item)
        {
            lock (SyncRoot)
            {
                base.InsertItem(index, item);
                if (!DisableEvents) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }

        }

        protected override void SetItem(int index, TElem item)
        {
            TElem oldItem;

            lock (SyncRoot)
            {
                if (index >= Count)
                {
                    InsertItem(0, item);
                    return;
                }

                oldItem = this[index];
                base.SetItem(index, item);

                if (!DisableEvents) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
            }
        }

        protected override void ClearItems()
        {
            lock (SyncRoot)
            {
                base.ClearItems();
                if (!DisableEvents) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected override void RemoveItem(int index)
        {
            TElem oldItem;

            lock (SyncRoot)
            {
                oldItem = this[index];
                base.RemoveItem(index);

                if (!DisableEvents) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
            }

        }

    }

}

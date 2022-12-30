using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace DataTools.Code.Markers
{
    /// <summary>
    /// Marker list, standard implementation.
    /// </summary>
    /// <typeparam name="TMarker">The <see cref="IMarker"/></typeparam>
    internal class MarkerList<TMarker> : Collection<TMarker>, IMarkerList<TMarker> where TMarker : IMarker
    {
        protected List<TMarker> List { get; }

        public MarkerList()
        {
            List = Items as List<TMarker>;
        }

        public MarkerList(IEnumerable<TMarker> items) : this()
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
    }

    /// <summary>
    /// Observable marker list, standard implementation.
    /// </summary>
    /// <typeparam name="TMarker">The <see cref="IMarker"/></typeparam>
    internal class ObservableMarkerList<TMarker> : MarkerList<TMarker>, IObservarbleMarkerList<TMarker> where TMarker : IMarker
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

        public ObservableMarkerList() : base()
        {
        }

        public ObservableMarkerList(IEnumerable<TMarker> items) : base(items)
        {
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

        protected override void InsertItem(int index, TMarker item)
        {
            lock (SyncRoot)
            {
                base.InsertItem(index, item);
                if (!DisableEvents) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
        }

        protected override void SetItem(int index, TMarker item)
        {
            TMarker oldItem;

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
            TMarker oldItem;

            lock (SyncRoot)
            {
                oldItem = this[index];
                base.RemoveItem(index);

                if (!DisableEvents) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
            }
        }
    }
}
﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
// ReSharper disable InheritdocConsiderUsage

namespace WixBuilder
{
    /// <summary>
    /// Provides a dictionary for use with data binding. This was originally in
    /// the parallel programming sample code (https://code.msdn.microsoft.com/ParExtSamples)
    /// and was modified to be used as a regular dictionary with change notification.
    /// </summary>
    /// <typeparam name="TKey">Specifies the type of the keys in this collection.</typeparam>
    /// <typeparam name="TValue">Specifies the type of the values in this collection.</typeparam>
    [DebuggerDisplay("Count={Count}")]
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        public ObservableDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Event raised when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        /// <summary>
        /// Event raised when a property on the collection changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies observers of CollectionChanged or PropertyChanged of an update to the dictionary.
        /// </summary>
        private void NotifyObserversOfChange()
        {
            var collectionHandler = CollectionChanged;
            var propertyHandler = PropertyChanged;
            if (collectionHandler == null && propertyHandler == null) return;
            collectionHandler?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            if (propertyHandler == null) return;
            propertyHandler(this, new PropertyChangedEventArgs("Count"));
            propertyHandler(this, new PropertyChangedEventArgs("Keys"));
            propertyHandler(this, new PropertyChangedEventArgs("Values"));
        }

        /// <summary>
        /// Attempts to add an item to the dictionary, notifying observers of any changes.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        /// <returns>Whether the add was successful.</returns>
        private void TryAddWithNotification(KeyValuePair<TKey, TValue> item) => TryAddWithNotification(item.Key, item.Value);

        /// <summary>
        /// Attempts to add an item to the dictionary, notifying observers of any changes.
        /// </summary>
        /// <param name="key">The key of the item to be added.</param>
        /// <param name="value">The value of the item to be added.</param>
        /// <returns>Whether the add was successful.</returns>
        private void TryAddWithNotification(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            NotifyObserversOfChange();
        }

        /// <summary>
        /// Attempts to remove an item from the dictionary, notifying observers of any changes.
        /// </summary>
        /// <param name="key">The key of the item to be removed.</param>
        /// <returns>Whether the removal was successful.</returns>
        private bool TryRemoveWithNotification(TKey key)
        {
            var result = _dictionary.Remove(key);
            if (result) NotifyObserversOfChange();
            return result;
        }

        /// <summary>
        /// Attempts to add or update an item in the dictionary, notifying observers of any changes.
        /// </summary>
        /// <param name="key">The key of the item to be updated.</param>
        /// <param name="value">The new value to set for the item.</param>
        /// <returns>Whether the update was successful.</returns>
        private void UpdateWithNotification(TKey key, TValue value)
        {
            _dictionary[key] = value;
            NotifyObserversOfChange();
        }

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => TryAddWithNotification(item);

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Clear();
            NotifyObserversOfChange();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<TKey, TValue>>.Count => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Count;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).IsReadOnly;

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => TryRemoveWithNotification(item.Key);

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).GetEnumerator();

        public IEnumerator GetEnumerator() => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).GetEnumerator();

        #endregion

        #region IDictionary<TKey,TValue> Members
        public void Add(TKey key, TValue value) => TryAddWithNotification(key, value);

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public ICollection<TKey> Keys => _dictionary.Keys;

        public bool Remove(TKey key) => TryRemoveWithNotification(key);

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        public ICollection<TValue> Values => _dictionary.Values;

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set => UpdateWithNotification(key, value);
        }
        #endregion

        public int Count => _dictionary.Count;
    }
}

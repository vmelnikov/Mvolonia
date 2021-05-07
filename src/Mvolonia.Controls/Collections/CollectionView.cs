using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Collections;

namespace Mvolonia.Controls.Collections
{
    public class CollectionView : ICollectionView, INotifyPropertyChanged
    {
        /// <summary>
        /// Private accessor for the InternalList
        /// </summary>
        private IList _internalList;

        private readonly CollectionViewGroupRoot _rootGroup;

        public CollectionView(IEnumerable source)
        {
            SourceCollection = source ?? throw new ArgumentNullException(nameof(source));

            _rootGroup = new CollectionViewGroupRoot(this);
            _rootGroup.GroupDescriptions.CollectionChanged += OnGroupByChanged;

            CopySourceToInternalList();

            // If we implement INotifyCollectionChanged
            if (source is INotifyCollectionChanged coll)
            {
                coll.CollectionChanged += (_, args) => ProcessCollectionChanged(args);
            }
        }

        public bool IsGrouping { get; private set; }

        public IEnumerable GroupingItems =>
            IsGrouping
                ? _rootGroup.Items
                : _internalList as IEnumerable;

        /// <summary>
        /// Gets the description of grouping, indexed by level.
        /// </summary>
        public AvaloniaList<GroupDescription> GroupDescriptions =>
            _rootGroup.GroupDescriptions;

        private IEnumerable SourceCollection { get; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnGroupByChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Refresh();
        }

        /// <summary>
        ///     Notify listeners that this View has changed
        /// </summary>
        /// <param name="args">
        ///     The NotifyCollectionChangedEventArgs to be passed to the EventHandler
        /// </param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args) =>
            CollectionChanged?.Invoke(this, args);


        private void ProcessCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            var addedItem = args.NewItems?[0];
            var removedItem = args.OldItems?[0];
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ProcessAddEvent(addedItem, args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ProcessRemoveEvent(removedItem);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ProcessReplaceEvent(removedItem, addedItem);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ProcessResetEvent();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            

            if (args.Action != NotifyCollectionChangedAction.Replace)
                OnPropertyChanged(nameof(Count));
            
        }

        private void ProcessReplaceEvent(object oldValue, object newValue)
        {
            throw new NotImplementedException();
        }

        private void ProcessResetEvent() =>
            Refresh();


        private void ProcessAddEvent(object value, int index)
        {
            var addedIndex = InsertToInternalList(value, index);
            _rootGroup.AddToSubgroups(value, false);
            
            if (addedIndex < 0)
                return;

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    value,
                    addedIndex));
        }

        private void ProcessRemoveEvent(object value)
        {
            var removedIndex = RemoveItemFromInternalList(value);
            _rootGroup.RemoveFromSubgroups(value);
            if (removedIndex < 0)
                return;

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    value,
                    removedIndex));
        }

        private int InsertToInternalList(object item, int index)
        {
            // make sure that the specified insert index is within the valid range
            // otherwise, just add it to the end. the index can be set to an invalid
            // value if the item was originally not in the collection, on a different
            // page, or if it had been previously filtered out.
            if (index < 0 || index > _internalList.Count)
                index = _internalList.Count;

            _internalList.Insert(index, item);
            return index;
        }

        private int RemoveItemFromInternalList(object value)
        {
            var index = _internalList.IndexOf(value);
            if (index >= 0)
                _internalList.Remove(value);
            return index;
        }

        private void Refresh()
        {
            CopySourceToInternalList();
            PrepareGroups();
        }

        private void PrepareGroups()
        {
            _rootGroup.Clear();
            _rootGroup.Initialize();


            // set to false so that we access internal collection items
            // instead of the group items, as they have been cleared
            IsGrouping = false;

            if (_rootGroup.GroupDescriptions.Count > 0)
            {
                for (int num = 0, count = _internalList.Count; num < count; ++num)
                {
                    var item = _internalList[num];
                    if (item != null)
                    {
                        _rootGroup.AddToSubgroups(item, loading: true);
                    }
                }
            }

            IsGrouping = _rootGroup.GroupBy != null;
        }


        /// <summary>
        /// Helper to raise a PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Property name for the property that changed</param>
        private void OnPropertyChanged(string propertyName) =>
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));


        private void OnPropertyChanged(PropertyChangedEventArgs e) =>
            PropertyChanged?.Invoke(this, e);


        public IEnumerator GetEnumerator() =>
            _internalList.GetEnumerator();


        /// <summary>
        /// Copy all items from the source collection to the internal list for processing.
        /// </summary>
        private void CopySourceToInternalList()
        {
            _internalList = new List<object>();

            var enumerator = SourceCollection.GetEnumerator();

            while (enumerator.MoveNext())
                _internalList.Add(enumerator.Current);
        }

        public int Count =>
            _internalList.Count;

        public void CopyTo(Array array, int index) =>
            _internalList.CopyTo(array, index);

        public bool IsSynchronized =>
            _internalList.IsSynchronized;

        public object SyncRoot =>
            _internalList.SyncRoot;

        public int Add(object value) =>
            throw new NotSupportedException();

        public void Clear() =>
            throw new NotSupportedException();

        public bool Contains(object value) =>
            _internalList.Contains(value);

        public int IndexOf(object value) =>
            _internalList.IndexOf(value);

        public void Insert(int index, object value) =>
            throw new NotSupportedException();

        public void Remove(object value) =>
            throw new NotSupportedException();

        public void RemoveAt(int index) =>
            throw new NotSupportedException();

        public bool IsFixedSize => true;
        public bool IsReadOnly => true;

        public object this[int index]
        {
            get => _internalList[index];
            set => throw new NotSupportedException();
        }
    }
}
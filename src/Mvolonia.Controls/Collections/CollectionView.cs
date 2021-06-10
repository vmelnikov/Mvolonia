using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Avalonia.Collections;
using Mvolonia.Controls.Collections.Comparers;

namespace Mvolonia.Controls.Collections
{
    public class CollectionView : ICollectionView, INotifyPropertyChanged
    {
        private IList _internalList;

        private readonly CollectionViewGroupRoot _rootGroup;
        private SortDescriptionCollection _sortDescriptions;

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

        private Type _itemType;

        private Type ItemType
        {
            get
            {
                //    if (_itemType == null)
                //        _itemType = GetItemType(true);

                return _itemType;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a private copy of the data 
        /// is needed for sorting. We want any deriving 
        /// classes to also be able to access this value to see whether or not 
        /// to use the default source collection, or the internal list.
        /// </summary>
        private bool UsesLocalArray =>
            SortDescriptions.Count > 0 || GroupDescriptions.Count > 0;


        CollectionViewGroup ICollectionView.FindGroupContainingItem(object item) =>
            FindGroupContainingItem(_rootGroup, item);


        private static CollectionViewGroup FindGroupContainingItem(CollectionViewGroup group, object item)
        {
            foreach (var groupItem in group.Items)
            {
                if (Equals(groupItem, item))
                    return group;
                if (!(groupItem is CollectionViewGroup collectionViewGroup))
                    continue;
                var subgroup = FindGroupContainingItem(collectionViewGroup, item);
                if (subgroup is null)
                    continue;
                return subgroup;
            }

            return null;
        }

        /// <summary>
        /// Gets the description of grouping, indexed by level.
        /// </summary>
        public AvaloniaList<GroupDescription> GroupDescriptions =>
            _rootGroup.GroupDescriptions;

        /// <summary>
        /// Collection of Sort criteria to sort items in this view over the SourceCollection.
        /// </summary>
        /// <remarks>
        /// <p>
        /// One or more sort criteria in form of <seealso cref="SortDescription"/>
        /// can be added, each specifying a property and direction to sort by.
        /// </p>
        /// </remarks>
        public SortDescriptionCollection SortDescriptions
        {
            get
            {
                if (_sortDescriptions is null)
                    SetSortDescriptions(new SortDescriptionCollection());
                return _sortDescriptions;
            }
        }

        private IEnumerable SourceCollection { get; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Set new SortDescription collection; re-hook collection change notification handler
        /// </summary>
        /// <param name="descriptions">SortDescriptionCollection to set the property addedItem to</param>
        private void SetSortDescriptions(SortDescriptionCollection descriptions)
        {
            if (_sortDescriptions != null)
                _sortDescriptions.CollectionChanged -= SortDescriptionsChanged;


            _sortDescriptions = descriptions;

            if (_sortDescriptions is null)
                return;
            Debug.Assert(_sortDescriptions.Count == 0, "must be empty SortDescription collection");
            _sortDescriptions.CollectionChanged += SortDescriptionsChanged;
        }

        private void SortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Refresh();
            OnPropertyChanged(nameof(SortDescriptions));
        }

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


        private void ProcessAddEvent(object item, int index)
        {
            ProcessInsertToCollection(item, index);

            PrepareGroupingComparer(_rootGroup);

            if (IsGrouping)
                _rootGroup.AddToSubgroups(item, false);

            index = IndexOf(item);

            if (index < 0)
                return;

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    item,
                    index));
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

        /// <summary>
        /// Handles adding an item into the collection, and applying sorting, filtering, grouping, paging.
        /// </summary>
        /// <param name="item">Item to insert in the collection</param>
        /// <param name="index">Index to insert item into</param>
        private void ProcessInsertToCollection(object item, int index)
        {
            // first check to see if it passes the filter
            if (SortDescriptions.Count > 0)
            {
                // var itemType = ItemType;
                // foreach (var sort in SortDescriptions)
                //     sort.Initialize(itemType);

                // create the SortFieldComparer to use
                var sortFieldComparer = new MergedComparer(this);

                // check if the item would be in sorted order if inserted into the specified index
                // otherwise, calculate the correct sorted index
                if (index < 0 || /* if item was not originally part of list */
                    (index > 0 &&
                     (sortFieldComparer.Compare(item, InternalItemAt(index - 1)) <
                      0)) || /* item has moved up in the list */
                    ((index < _internalList.Count - 1) &&
                     (sortFieldComparer.Compare(item, InternalItemAt(index)) >
                      0))) /* item has moved down in the list */
                {
                    index = sortFieldComparer.FindInsertIndex(item, _internalList);
                }
            }

            // make sure that the specified insert index is within the valid range
            // otherwise, just add it to the end. the index can be set to an invalid
            // item if the item was originally not in the collection, on a different
            // page, or if it had been previously filtered out.
            if (index < 0 || index > _internalList.Count)
            {
                index = _internalList.Count;
            }

            _internalList.Insert(index, item);
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
            IsGrouping = false;
            CopySourceToInternalList();
            if (!UsesLocalArray)
                return;
            SortInternalList();
            PrepareGroups();
        }

        /// <summary>
        /// Sort the List based on the SortDescriptions property.
        /// </summary>
        private void SortInternalList()
        {
            var seq = (IEnumerable<object>) _internalList;
            var itemType = ItemType;

            foreach (var sort in SortDescriptions)
            {
                // sort.Initialize(itemType); 

                if (seq is IOrderedEnumerable<object> orderedEnum)
                    seq = sort.ThenBy(orderedEnum);
                else
                    seq = sort.OrderBy(seq);
            }

            _internalList = seq.ToList();
        }


        /// <summary>
        /// Sets up the ActiveComparer for the CollectionViewGroupRoot specified
        /// </summary>
        /// <param name="groupRoot">The CollectionViewGroupRoot</param>
        private void PrepareGroupingComparer(CollectionViewGroupRoot groupRoot)
        {
            if (groupRoot.ActiveComparer is ListComparer listComparer)
            {
                listComparer.ResetList(_internalList);
                return;
            }

            groupRoot.ActiveComparer = new ListComparer(_internalList);
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
            PrepareGroupingComparer(_rootGroup);
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
            IsGrouping
                ? _rootGroup.GetLeafEnumerator()
                : _internalList.GetEnumerator();


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


        /// <summary>
        /// Return item at the given index in the internal list.
        /// </summary>
        /// <param name="index">The index we are checking</param>
        /// <returns>The item at the specified index</returns>
        private object InternalItemAt(int index) =>
            index >= 0 && index < _internalList.Count ? _internalList[index] : null;

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
            IsGrouping
                ? _rootGroup.LeafIndexOf(value)
                : _internalList.IndexOf(value);

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
            get =>  IsGrouping 
                ? _rootGroup.LeafAt(index)
                : _internalList[index];
            set => throw new NotSupportedException();
        }
    }
}
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

        /// <summary>
        /// Gets the minimum number of items known to be in the source collection
        /// that verify the current filter if any
        /// </summary>
        public int ItemCount => 
             IsGrouping 
                ? _rootGroup.ItemCount
                : _internalList.Count;

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

        /// <summary>
        /// Gets a value indicating whether a private copy of the data 
        /// is needed for grouping
        /// </summary>
        private bool UsesLocalArray => GroupDescriptions.Count > 0;


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
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged is null)
                return;
            CollectionChanged(this, args);
            
        }

        private void ProcessCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                // if we have no items now, clear our own internal list
                if (!SourceCollection.GetEnumerator().MoveNext())
                    _internalList.Clear();
                

                // calling Refresh, will fire the CollectionChanged event
                Refresh();
                return;
            }

            var addedItem = args.NewItems?[0];
            var removedItem = args.OldItems?[0];

            // fire notifications for removes
            if (args.Action == NotifyCollectionChangedAction.Remove ||
                args.Action == NotifyCollectionChangedAction.Replace)
            {
                ProcessRemoveEvent(removedItem, args.Action == NotifyCollectionChangedAction.Replace);
            }

            // fire notifications for adds
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                ProcessAddEvent(addedItem, args.NewStartingIndex);
            }
            if (args.Action != NotifyCollectionChangedAction.Replace)
            {
                OnPropertyChanged(nameof(ItemCount));
            }
        }

        private void ProcessAddEvent(object addedItem, int addIndex)
        {
            ProcessInsertToCollection(addedItem, addIndex);
            _rootGroup.AddToSubgroups(addedItem, false);
            
            var addedIndex = _rootGroup?.LeafIndexOf(addedItem) ?? -1;
            
            // if the item is within the current page
            if (addedIndex >= 0)
            {
                // fire add notification
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        addedItem,
                        addedIndex));
            }
        }

        private void ProcessRemoveEvent(object removedItem, bool b)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Handles adding an item into the collection, and applying sorting, filtering, grouping, paging.
        /// </summary>
        /// <param name="item">Item to insert in the collection</param>
        /// <param name="index">Index to insert item into</param>
        private void ProcessInsertToCollection(object item, int index)
        {
          

                // make sure that the specified insert index is within the valid range
                // otherwise, just add it to the end. the index can be set to an invalid
                // value if the item was originally not in the collection, on a different
                // page, or if it had been previously filtered out.
                if (index < 0 || index > _internalList.Count)
                {
                    index = _internalList.Count;
                }

                _internalList.Insert(index, item);
            
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
        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public IEnumerator GetEnumerator()
        {
            return IsGrouping 
                ? _rootGroup.GetLeafEnumerator() 
                : _internalList.GetEnumerator();
        }
        
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

        
    }
}
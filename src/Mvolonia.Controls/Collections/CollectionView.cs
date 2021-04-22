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
        public int ItemCount => _internalList.Count;

        public bool IsGrouping { get; private set; }

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


        public event PropertyChangedEventHandler PropertyChanged;

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

        private void ProcessAddEvent(object addedItem, int argsNewStartingIndex)
        {
            throw new NotImplementedException();
        }

        private void ProcessRemoveEvent(object removedItem, bool b)
        {
            throw new NotImplementedException();
        }

        private void Refresh()
        {
            // set IsGrouping to false
             IsGrouping = false;
             
             CopySourceToInternalList();
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
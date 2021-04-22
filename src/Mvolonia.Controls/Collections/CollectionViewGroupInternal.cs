using System.Collections;
using System.ComponentModel;

namespace Mvolonia.Controls.Collections
{
    internal class CollectionViewGroupInternal : CollectionViewGroup
    {
        private GroupDescription _groupBy;

        internal CollectionViewGroupInternal(object key, CollectionViewGroupInternal parent) : base(key)
        {
            Parent = parent;
        }
        
        /// <summary>
        /// Returns an enumerator over the leaves governed by this group
        /// </summary>
        /// <returns>Enumerator of leaves</returns>
        internal IEnumerator GetLeafEnumerator() =>
            new LeafEnumerator(this);

        private CollectionViewGroupInternal Parent { get; }

        internal int FullCount { get; set; }
        
        internal GroupDescription GroupBy
        {
            get => _groupBy; 
            set
            {
                if (_groupBy != null)
                    ((INotifyPropertyChanged)_groupBy).PropertyChanged -= OnGroupByChanged;
                
                _groupBy = value;

                if (_groupBy != null)
                    ((INotifyPropertyChanged)_groupBy).PropertyChanged += OnGroupByChanged;
            }
        }

        /// <summary>
        /// Gets or sets the most recent index where activity took place
        /// </summary>
        internal int LastIndex { get; set; }

        private void OnGroupByChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
        
        /// <summary>
        /// Update the item count of the CollectionViewGroup
        /// </summary>
        /// <param name="item">CollectionViewGroup to update</param>
        /// <param name="delta">Delta to change count by</param>
        protected void ChangeCounts(object item, int delta)
        {
            var changeLeafCount = !(item is CollectionViewGroup);

            for (var group = this;
                group != null;
                group = group.Parent)
            {
                group.FullCount += delta;
                if (!changeLeafCount) 
                    continue;
                
                group.ProtectedItemCount += delta;

                if (group.ProtectedItemCount == 0)
                    RemoveEmptyGroup(group);
            }
            
        }

        /// <summary>
        /// Removes an empty group from the PagedCollectionView grouping
        /// </summary>
        /// <param name="group">Empty subgroup to remove</param>
        private static void RemoveEmptyGroup(CollectionViewGroupInternal group)
        {
            var parent = group.Parent;

            if (parent is null)
                return;

            var groupBy = parent.GroupBy;
            var index = parent.ProtectedItems.IndexOf(group);

            // remove the subgroup unless it is one of the explicit groups
            if (index >= groupBy.GroupKeys.Count)
                parent.Remove(group, false); 
        }

        /// <summary>
        /// Removes the specified item from the collection
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <param name="returnLeafIndex">Whether we want to return the leaf index</param>
        /// <returns>Leaf index where item was removed, if value was specified. Otherwise '-1'</returns>
        internal int Remove(object item, bool returnLeafIndex)
        {
            var index = -1;
            var localIndex = ProtectedItems.IndexOf(item);

            if (localIndex < 0) 
                return index;
            if (returnLeafIndex)
                index = LeafIndexFromItem(null, localIndex);
            
            ChangeCounts(item, -1);
            ProtectedItems.RemoveAt(localIndex);
            return index;
        }

        /// <summary>
        /// Adds the specified item to the collection
        /// </summary>
        /// <param name="item">Item to add</param>
        internal void Add(object item)
        {
            ChangeCounts(item, +1);
            ProtectedItems.Add(item);
        }
        
        
        /// <summary>
        /// Returns the index of the given item within the list of leaves governed
        /// by the full group structure.  The item must be a (direct) child of this
        /// group.  The caller provides the index of the item within this group,
        /// if known, or -1 if not.
        /// </summary>
        /// <param name="item">Item we are looking for</param>
        /// <param name="index">Index of the leaf</param>
        /// <returns>Number of items under that leaf</returns>
        internal int LeafIndexFromItem(object item, int index)
        {
            int result = 0;

            // accumulate the number of predecessors at each level
            for (var group = this;
                group != null;
                item = group, group = group.Parent, index = -1)
            {
                // accumulate the number of predecessors at the level of item
                for (int k = 0, n = group.Items.Count; k < n; ++k)
                {
                    // if we've reached the item, move up to the next level
                    if ((index < 0 && Equals(item, group.Items[k])) ||
                        index == k)
                    {
                        break;
                    }

                    // accumulate leaf count
                    var subgroup = group.Items[k] as CollectionViewGroupInternal;
                    result += subgroup?.ItemCount ?? 1;
                }
            }

            return result;
        }
    }
}
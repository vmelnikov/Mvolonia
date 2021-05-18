using System.Collections;
using System.ComponentModel;
using Avalonia;

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

        internal CollectionViewGroupInternal Parent { get; }

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
        
        /// <summary>
        /// Gets the first item (leaf) added to this group.  If this can't be determined,
        /// DependencyProperty.UnsetValue.
        /// </summary>
        internal object SeedItem
        {
            get
            {
                if (ItemCount > 0 && (GroupBy is null || GroupBy.GroupKeys.Count == 0))
                {
                    // look for first item, child by child
                    for (int k = 0, n = Items.Count; k < n; ++k)
                    {
                        if (!(Items[k] is CollectionViewGroupInternal subgroup))
                        {
                            // child is an item - return it
                            return Items[k];
                        }
                        else if (subgroup.ItemCount > 0)
                        {
                            // child is a nonempty subgroup - ask it
                            return subgroup.SeedItem;
                        }
                        //// otherwise child is an empty subgroup - go to next child
                    }

                    // we shouldn't get here, but just in case...

                    return AvaloniaProperty.UnsetValue;
                }
                else
                {
                    // the group is empty, or it has explicit subgroups.
                    // In either case, we cannot determine the first item -
                    // it could have gone into any of the subgroups.
                    return AvaloniaProperty.UnsetValue;
                }
            }
        }

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
        /// Insert a new item or subgroup and return its index.  Seed is a
        /// representative from the subgroup (or the item itself) that
        /// is used to position the new item/subgroup w.r.t. the order given
        /// by the comparer. (If comparer is null, just add at the end).
        /// </summary>
        /// <param name="item">Item we are looking for</param>
        /// <param name="seed">Seed of the item we are looking for</param>
        /// <param name="comparer">Comparer used to find the item</param>
        /// <returns>The index where the item was inserted</returns>
        internal int Insert(object item, object seed, IComparer comparer)
        {
            // never insert the new item/group before the explicit subgroups
            var low = GroupBy?.GroupKeys.Count ?? 0;
            int index = FindIndex(item, seed, comparer, low, ProtectedItems.Count);

            // now insert the item
            ChangeCounts(item, +1);
            ProtectedItems.Insert(index, item);

            return index;
        }
        
        /// <summary>
        /// Clears the collection of items
        /// </summary>
        internal void Clear()
        {
            ProtectedItems.Clear();
            FullCount = 1;
            ProtectedItemCount = 0;
        }
        
        /// <summary>
        /// Finds the index of the specified item
        /// </summary>
        /// <param name="item">Item we are looking for</param>
        /// <param name="seed">Seed of the item we are looking for</param>
        /// <param name="comparer">Comparer used to find the item</param>
        /// <param name="low">Low range of item index</param>
        /// <param name="high">High range of item index</param>
        /// <returns>Index of the specified item</returns>
        protected virtual int FindIndex(object item, object seed, IComparer comparer, int low, int high)
        {
            int index;

            if (comparer != null)
            {
                // if (comparer is ListComparer listComparer)
                // {
                //     // reset the IListComparer before each search. This cannot be done
                //     // any less frequently (e.g. in Root.AddToSubgroups), due to the
                //     // possibility that the item may appear in more than one subgroup.
                //     listComparer.Reset();
                // }
                //
                // if (comparer is CollectionViewGroupComparer groupComparer)
                // {
                //     // reset the CollectionViewGroupComparer before each search. This cannot be done
                //     // any less frequently (e.g. in Root.AddToSubgroups), due to the
                //     // possibility that the item may appear in more than one subgroup.
                //     groupComparer.Reset();
                // }

                for (index = low; index < high; ++index)
                {
                    object seed1 = (ProtectedItems[index] is CollectionViewGroupInternal subgroup) ? subgroup.SeedItem : ProtectedItems[index];
                    if (seed1 == AvaloniaProperty.UnsetValue)
                    {
                        continue;
                    }
                    if (comparer.Compare(seed, seed1) < 0)
                    {
                        break;
                    }
                }
            }
            else
            {
                index = high;
            }

            return index;
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
        
        /// <summary>
        /// Returns the index of the given item within the list of leaves governed
        /// by this group
        /// </summary>
        /// <param name="item">Item we are looking for</param>
        /// <returns>Number of items under that leaf</returns>
        internal int LeafIndexOf(object item)
        {
            var leaves = 0;         // number of leaves we've passed over so far
            for (int k = 0, n = Items.Count; k < n; ++k)
            {
                if (Items[k] is CollectionViewGroupInternal subgroup)
                {
                    var subgroupIndex = subgroup.LeafIndexOf(item);
                    if (subgroupIndex < 0)
                    {
                        leaves += subgroup.ItemCount;       // item not in this subgroup
                    }
                    else
                    {
                        return leaves + subgroupIndex;    // item is in this subgroup
                    }
                }
                else
                {
                    // current item is a leaf - compare it directly
                    
                    
                    if (Equals(item, Items[k]))
                    {
                        return leaves;
                    }

                    leaves += 1;
                }
            }

            // item not found
            return -1;
        }
    }
}
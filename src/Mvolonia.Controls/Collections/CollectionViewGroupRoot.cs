using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia.Collections;

namespace Mvolonia.Controls.Collections
{
    internal class CollectionViewGroupRoot : CollectionViewGroupInternal
    {
        /// <summary>
        /// String constant used for the Root Name
        /// </summary>
        private const string RootName = "Root";

        private readonly ICollectionView _view;

        /// <summary>
        /// Private accessor for empty object instance
        /// </summary>
        private static readonly object UseAsItemDirectly = new object();

        /// <summary>
        /// Private accessor for an ObservableCollection containing group descriptions
        /// </summary>
        private readonly AvaloniaList<GroupDescription> _groupBy = new AvaloniaList<GroupDescription>();

        public CollectionViewGroupRoot(ICollectionView view) : base(RootName, null)
        {
            _view = view;
        }

        /// <summary>
        /// Raise this event when the (grouped) view changes
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Gets the description of grouping, indexed by level.
        /// </summary>
        public AvaloniaList<GroupDescription> GroupDescriptions => _groupBy;

        /// <summary>
        /// Gets or sets the current IComparer being used
        /// </summary>
        internal IComparer ActiveComparer { get; set; }


        /// <summary>
        /// Initializes the group descriptions
        /// </summary>
        internal void Initialize() =>
            InitializeGroup(this, 0, null);

        /// <summary>
        /// Adds specified item to subgroups
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="loading">Whether we are currently loading</param>
        internal void AddToSubgroups(object item, bool loading)
        {
            AddToSubgroups(item, this, 0, loading);
        }

        /// <summary>
        /// Add an item to the desired subgroup(s) of the given group
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="group">Group to add item to</param>
        /// <param name="level">The level of grouping</param>
        /// <param name="loading">Whether we are currently loading</param>
        private void AddToSubgroups(object item, CollectionViewGroupInternal group, int level, bool loading)
        {
            var key = GetGroupKey(item, group.GroupBy, level);

            if (key == UseAsItemDirectly)
            {
                // the item belongs to the group itself (not to any subgroups)
                if (loading)
                {
                    group.Add(item);
                }
                else
                {
                    var localIndex = group.Insert(item, item, ActiveComparer);
                    var index = group.LeafIndexFromItem(item, localIndex);
                    OnCollectionChanged(
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                }
            }
            else if (key is ICollection keyList)
            {
                // the item belongs to multiple subgroups
                foreach (var o in keyList)
                    AddToSubgroup(item, group, level, o, loading);
            }
            else
            {
                // the item belongs to one subgroup
                AddToSubgroup(item, group, level, key, loading);
            }
        }

        /// <summary>
        /// Add an item to the subgroup with the given name
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="group">Group to add item to</param>
        /// <param name="level">The level of grouping.</param>
        /// <param name="key">Name of subgroup to add to</param>
        /// <param name="loading">Whether we are currently loading</param>
        private void AddToSubgroup(object item, CollectionViewGroupInternal group, int level, object key, bool loading)
        {
            CollectionViewGroupInternal subgroup;
            var index = 0;

            // find the desired subgroup
            for (var n = group.Items.Count; index < n; ++index)
            {
                subgroup = group.Items[index] as CollectionViewGroupInternal;
                if (subgroup == null)
                {
                    continue; // skip children that are not groups
                }

                if (group.GroupBy.KeysMatch(subgroup.Key, key))
                {
                    group.LastIndex = index;
                    AddToSubgroups(item, subgroup, level + 1, loading);
                    return;
                }
            }

            // the item didn't match any subgroups.  Create a new subgroup and add the item.
            subgroup = new CollectionViewGroupInternal(key, group);
            InitializeGroup(subgroup, level + 1, item);

            if (loading)
            {
                group.Add(subgroup);
                group.LastIndex = index;
            }
            else
            {
                // using insert will find the correct sort index to
                // place the subgroup, and will default to the last
                // position if no ActiveComparer is specified
                group.Insert(subgroup, item, ActiveComparer);
            }

            AddToSubgroups(item, subgroup, level + 1, loading);
        }

        /// <summary>
        /// Remove specified item from subgroups
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <returns>Whether the operation was successful</returns>
        internal bool RemoveFromSubgroups(object item)
        {
            return RemoveFromSubgroups(item, this, 0);
        }

        /// <summary>
        /// Remove an item from the desired subgroup(s) of the given group.
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <param name="group">Group to remove item from</param>
        /// <param name="level">The level of grouping</param>
        /// <returns>Return true if the item was not in one of the subgroups it was supposed to be.</returns>
        private bool RemoveFromSubgroups(object item, CollectionViewGroupInternal group, int level)
        {
            var itemIsMissing = false;
            var key = GetGroupKey(item, group.GroupBy, level);

            if (key == UseAsItemDirectly)
            {
                // the item belongs to the group itself (not to any subgroups)
                itemIsMissing = RemoveFromGroupDirectly(group, item);
            }
            else if (key is ICollection keyList)
            {
                // the item belongs to multiple subgroups
                foreach (var o in keyList)
                {
                    if (RemoveFromSubgroup(item, group, level, o))
                    {
                        itemIsMissing = true;
                    }
                }
            }
            else
            {
                // the item belongs to one subgroup
                if (RemoveFromSubgroup(item, group, level, key))
                {
                    itemIsMissing = true;
                }
            }

            return itemIsMissing;
        }

        /// <summary>
        /// Remove an item from the subgroup with the given name.
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <param name="group">Group to remove item from</param>
        /// <param name="level">The level of grouping</param>
        /// <param name="key">Name of item to remove</param>
        /// <returns>Return true if the item was not in one of the subgroups it was supposed to be.</returns>
        private bool RemoveFromSubgroup(object item, CollectionViewGroupInternal group, int level, object key)
        {
            var itemIsMissing = false;

            // find the desired subgroup
            for (int index = 0, n = group.Items.Count; index < n; ++index)
            {
                if (!(group.Items[index] is CollectionViewGroupInternal subgroup))
                    continue; // skip children that are not groups

                if (!group.GroupBy.KeysMatch(subgroup.Key, key))
                    continue;

                if (RemoveFromSubgroups(item, subgroup, level + 1))
                    itemIsMissing = true;

                return itemIsMissing;
            }

            // the item didn't match any subgroups.  It should have.
            return true;
        }

        /// <summary>
        /// Remove an item from the direct children of a group.
        /// </summary>
        /// <param name="group">Group to remove item from</param>
        /// <param name="item">Item to remove</param>
        /// <returns>True if item could not be removed</returns>
        private bool RemoveFromGroupDirectly(CollectionViewGroupInternal group, object item)
        {
            var leafIndex = group.Remove(item, true);
            if (leafIndex < 0)
                return true;

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, leafIndex));
            return false;
        }


        /// <summary>
        /// Get the group name(s) for the given item
        /// </summary>
        /// <param name="item">Item to get group name for</param>
        /// <param name="groupDescription">GroupDescription for the group</param>
        /// <param name="level">The level of grouping</param>
        /// <returns>Group names for the specified item</returns>
        private static object GetGroupKey(object item, GroupDescription groupDescription, int level)
        {
            return groupDescription is null
                ? UseAsItemDirectly
                : groupDescription.GroupKeyFromItem(item, level);
        }

        /// <summary>
        /// Initialize the given group
        /// </summary>
        /// <param name="group">Group to initialize</param>
        /// <param name="level">The level of grouping</param>
        /// <param name="seedItem">The seed item to compare with to see where to insert</param>
        private void InitializeGroup(CollectionViewGroupInternal group, int level, object seedItem)
        {
            // set the group description for dividing the group into subgroups
            var groupDescription = GetGroupDescription(group, level);
            group.GroupBy = groupDescription;

            // create subgroups for each of the explicit names
            var keys = groupDescription?.GroupNames;
            if (keys != null)
            {
                for (int k = 0, n = keys.Count; k < n; ++k)
                {
                    var subGroup = new CollectionViewGroupInternal(keys[k], group);
                    InitializeGroup(subGroup, level + 1, seedItem);
                    group.Add(subGroup);
                }
            }

            group.LastIndex = 0;
        }


        public virtual Func<CollectionViewGroup, int, GroupDescription> GroupBySelector { get; set; }

        /// <summary>
        /// Returns the description of how to divide the given group into subgroups
        /// </summary>
        /// <param name="group">CollectionViewGroup to get group description from</param>
        /// <param name="level">The level of grouping</param>
        /// <returns>GroupDescription of how to divide the given group</returns>
        private GroupDescription GetGroupDescription(CollectionViewGroup group, int level)
        {
            GroupDescription result = null;
            if (Equals(group, this))
                group = null;


            if (!(GroupBySelector is null))
                result = GroupBySelector.Invoke(group, level);


            if (result is null && level < GroupDescriptions.Count)
                result = GroupDescriptions[level];


            return result;
        }

        /// <summary>
        /// Notify listeners that this View has changed
        /// </summary>
        /// <param name="args">The NotifyCollectionChangedEventArgs to be passed to the EventHandler</param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }
    }
}
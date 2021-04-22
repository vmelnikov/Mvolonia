using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Collections;

namespace Mvolonia.Controls.Collections
{
    
    internal class CollectionViewGroupRoot : CollectionViewGroupInternal
    {
        /// <summary>
        /// Private accessor for the top level GroupDescription
        /// </summary>
        private static GroupDescription _topLevelGroupDescription = new TopLevelGroupDescription();
        
        /// <summary>
        /// String constant used for the Root Name
        /// </summary>
        private const string RootName = "Root";

        private readonly ICollectionView _view;
        private AvaloniaList<GroupDescription> _groupBy;

        public CollectionViewGroupRoot(ICollectionView view) : base(RootName, null)
        {
            _view = view;
        }
        
        /// <summary>
        /// Gets the description of grouping, indexed by level.
        /// </summary>
        public AvaloniaList<GroupDescription> GroupDescriptions => _groupBy;
        
        
        
        /// <summary>
        /// Initializes the group descriptions
        /// </summary>
        internal void Initialize() =>
            InitializeGroup(this, 0, null);

        /// <summary>
        /// Initialize the given group
        /// </summary>
        /// <param name="group">Group to initialize</param>
        /// <param name="level">The level of grouping</param>
        /// <param name="seedItem">The seed item to compare with to see where to insert</param>
        private void InitializeGroup(CollectionViewGroupInternal group, int level, object seedItem)
        {
            // set the group description for dividing the group into subgroups
            GroupDescription groupDescription = GetGroupDescription(group, level);
            group.GroupBy = groupDescription;

            // create subgroups for each of the explicit names
            var keys = groupDescription?.GroupKeys;
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
        /// TopLevelGroupDescription class
        /// </summary>
        private class TopLevelGroupDescription : GroupDescription
        {
            /// <summary>
            /// Initializes a new instance of the TopLevelGroupDescription class.
            /// </summary>
            public TopLevelGroupDescription()
            {
            }

            /// <summary>
            /// We have to implement this abstract method, but it should never be called
            /// </summary>
            /// <param name="item">Item to get group name from</param>
            /// <param name="level">The level of grouping</param>
            /// <param name="culture">Culture used for sorting</param>
            /// <returns>We do not return a value here</returns>
            public override object GroupKeyFromItem(object item, int level, CultureInfo culture)
            {
                Debug.Assert(true, "We have to implement this abstract method, but it should never be called");
                return null;
            }
        }
    }
}
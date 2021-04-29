using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Presenters;
using Mvolonia.Controls.Collections;

namespace Mvolonia.Controls.Presenters
{
    public class GroupingItemsVirtualizerNone : ItemVirtualizerBase
    {
        public GroupingItemsVirtualizerNone(IItemsPresenter owner)
            : base(owner)
        {
            if (Items != null && owner.Panel != null)
            {
                AddContainers(0, Items);
            }
        }

        /// <inheritdoc/>
        public override bool IsLogicalScrollEnabled => false;

        /// <summary>
        /// This property should never be accessed because <see cref="IsLogicalScrollEnabled"/> is
        /// false.
        /// </summary>
        public override double ExtentValue
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// This property should never be accessed because <see cref="IsLogicalScrollEnabled"/> is
        /// false.
        /// </summary>
        public override double OffsetValue
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// This property should never be accessed because <see cref="IsLogicalScrollEnabled"/> is
        /// false.
        /// </summary>
        public override double ViewportValue
        {
            get { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public override void ItemsChanged(IEnumerable items, NotifyCollectionChangedEventArgs e)
        {
            base.ItemsChanged(items, e);
            ItemContainerSync.ItemsChanged(Owner, items, e);
            Owner.InvalidateMeasure();
        }

        /// <summary>
        /// Scrolls the specified item into view.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        public override void ScrollIntoView(int index)
        {
            if (index != -1)
            {
                var container = ItemContainerGenerator.ContainerFromIndex(index);
                container?.BringIntoView();
            }
        }

        private IList<ItemContainerInfo> AddContainers(int index, IEnumerable items)
        {
            var generator = ItemContainerGenerator;
            var result = new List<ItemContainerInfo>();
            var panel = Owner.Panel;

            foreach (var item in items)
            {
                var materialized = generator.Materialize(index++, item);
                
                if (Items is ICollectionView collectionView)
                {
                    var group = collectionView.GroupingItems.Cast<CollectionViewGroup>()
                        .FirstOrDefault(g => g.Items.Contains(item));
                    if (!(group is null))
                    {
                        var groupItem = panel.Children.OfType<GroupItem>()
                            .FirstOrDefault(c => Equals(c.ViewGroup, group));
                                
                        if (groupItem is null)
                        {
                            groupItem = new GroupItem
                            {
                                ViewGroup = group
                            };
                            panel.Children.Add(groupItem);
                        }

                        groupItem.Panel.Children.Add(materialized.ContainerControl);
                    }
                }
                else
                {
                    panel.Children.Add(materialized.ContainerControl);
                }

                // if (i.ContainerControl != null)
                // {
                //     if (i.Index < panel.Children.Count)
                //     {
                //         // TODO: This will insert at the wrong place when there are null items.
                //         panel.Children.Insert(i.Index, i.ContainerControl);
                //     }
                //     else
                //     {
                //         panel.Children.Add(i.ContainerControl);
                //     }
                // }

                result.Add(materialized);
            }

            return result;
        }

        private void RemoveContainers(IEnumerable<ItemContainerInfo> items)
        {
            var panel = Owner.Panel;

            foreach (var i in items)
            {
                if (i.ContainerControl != null)
                {
                    panel.Children.Remove(i.ContainerControl);
                }
            }
        }
    }
}
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
                ItemContainerSync.FillExplicitGroupItems(Owner);
                ItemContainerSync.AddContainers(Owner, 0, Items);
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
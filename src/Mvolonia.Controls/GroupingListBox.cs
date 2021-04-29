using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Mvolonia.Controls.Generators;
using Mvolonia.Controls.Utils;

namespace Mvolonia.Controls
{
    public class GroupingListBox : ListBox
    {
        
        /// <inheritdoc/>
        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            return new GroupItemContainerGenerator(
                this, 
                ListBoxItem.ContentProperty,
                ListBoxItem.ContentTemplateProperty);
        }

        protected override void OnContainersMaterialized(ItemContainerEventArgs e)
        {
            foreach (var container in e.Containers)
            {
                if (!(container.ContainerControl is GroupItem))
                    base.OnContainersMaterialized(new ItemContainerEventArgs(container));
            }
        }
        
        public void ChildContainersMaterialized(ItemContainerEventArgs e)
        {
            foreach (var container in e.Containers)
                OnContainersMaterialized(new ItemContainerEventArgs(new ItemContainerInfo(container.ContainerControl, container.Item, Items.IndexOf(container.Item))));
            
        }

        public void ChildContainersRecycled(ItemContainerEventArgs e)
        {
            foreach (var container in e.Containers)
                OnContainersRecycled(new ItemContainerEventArgs(new ItemContainerInfo(container.ContainerControl, container.Item, Items.IndexOf(container.Item))));
        }

        public void ChildContainersDematerialized(ItemContainerEventArgs e)
        {
            foreach (var container in e.Containers)
                OnContainersDematerialized(new ItemContainerEventArgs(new ItemContainerInfo(container.ContainerControl, container.Item, Items.IndexOf(container.Item))));
        }
    }
}
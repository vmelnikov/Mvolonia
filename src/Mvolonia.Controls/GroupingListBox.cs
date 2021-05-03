using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Mvolonia.Controls.Generators;
using Mvolonia.Controls.Utils;

namespace Mvolonia.Controls
{

    public class GroupingDataTemplate : DataTemplate
    {
        
    }
    public class GroupingListBox : ListBox
    {
        /// <summary>
        /// The collection of GroupStyle objects that describes the display of
        /// each level of grouping.  The entry at index 0 describes the top level
        /// groups, the entry at index 1 describes the next level, and so forth.
        /// If there are more levels of grouping than entries in the collection,
        /// the last entry is used for the extra levels.
        /// </summary>
        public ObservableCollection<GroupStyle> GroupStyle { get; } = new ObservableCollection<GroupStyle>();

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
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Mvolonia.Controls.Generators;

namespace Mvolonia.Controls
{
    
    public class GroupableListBox : ListBox, IGroupableItemsGeneratorHost
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
                ContentControl.ContentProperty,
                ContentControl.ContentTemplateProperty);
        }

        protected override void OnContainersMaterialized(ItemContainerEventArgs e)
        {
            foreach (var container in e.Containers)
            {
                if (!(container.ContainerControl is GroupItem))
                    base.OnContainersMaterialized(new ItemContainerEventArgs(container));
            }
        }

        public GroupStyle GetGroupStyle(int level)
        {
            // use last entry for all higher levels
            if (level >= GroupStyle.Count)
                level = GroupStyle.Count - 1;

            return (level >= 0) ? GroupStyle[level] : null;
        }
    }
}
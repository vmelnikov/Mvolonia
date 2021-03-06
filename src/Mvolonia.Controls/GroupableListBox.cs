using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Input;
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
        public ObservableCollection<GroupStyle> GroupStyle { get; } = new();
     
        
        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            return new GroupItemContainerGenerator(
                this, 
                ContentControl.ContentProperty,
                ContentControl.ContentTemplateProperty);
        }
        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = ProcessKeyDown(e);
            }
            base.OnKeyDown(e);
        }

        private bool ProcessKeyDown(KeyEventArgs e)
        {
            var focus = FocusManager.Instance;
            var direction = e.Key.ToNavigationDirection();
                
            if (focus.Current is null ||
                direction is null ||
                direction.Value.IsTab())
            {
                return false;
            }
                
            var current = GetListBoxItem(focus.Current as IControl);
                
            var index = ItemContainerGenerator?.IndexFromContainer(current) ?? -1;
            if (index == -1)
                return false;
                
            var next = GetNextControlFromGenerator(direction.Value, index);
            if (next is null)
                return false;
            focus.Focus(next, NavigationMethod.Directional, e.KeyModifiers);
            return true;
        }

        private static IControl GetListBoxItem(IControl current)
        {
            while (current is not null)
            {
                if (current is ListBoxItem)
                    return current;
                current = current.Parent;
            }

            return null;
        }

        private IInputElement GetNextControlFromGenerator(NavigationDirection direction, int index)
        {
            if (ItemContainerGenerator is null)
                return null;
            var nextIndex = direction switch
            {
                NavigationDirection.Down => index + 1,
                NavigationDirection.Up => index - 1,
                   _ => index
            };
            return ItemContainerGenerator?.ContainerFromIndex(nextIndex);
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
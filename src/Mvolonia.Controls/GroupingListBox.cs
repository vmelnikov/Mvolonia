using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Mvolonia.Controls.Generators;

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

        
    }
}
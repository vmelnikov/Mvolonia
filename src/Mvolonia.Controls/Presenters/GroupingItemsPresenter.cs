using System.Collections;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Presenters;

namespace Mvolonia.Controls.Presenters
{
    public class GroupingItemsPresenter :ItemsPresenter
    {

        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            var i = TemplatedParent as ItemsControl;
            var result = i?.ItemContainerGenerator;
            
            if (result == null)
            {
                var groupItem = TemplatedParent as GroupItem;
                result = groupItem?.ItemContainerGenerator;
            }
            
            if (result == null)
            {
                result = new ItemContainerGenerator(this);
                result.ItemTemplate = ItemTemplate;
            }

            return result;
        }
    }
}
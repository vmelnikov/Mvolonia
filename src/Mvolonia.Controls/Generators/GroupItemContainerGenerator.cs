using System.Xml.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Mvolonia.Controls.Collections;

namespace Mvolonia.Controls.Generators
{
    public class GroupItemContainerGenerator : ItemContainerGenerator<ListBoxItem>
    {
        public GroupItemContainerGenerator(IControl owner, AvaloniaProperty contentProperty, AvaloniaProperty contentTemplateProperty) : base(owner, contentProperty, contentTemplateProperty)
        {
        }
            
        protected override IControl CreateContainer(object item)
        {
            return base.CreateContainer(item);
            // if (!(item is CollectionViewGroup  collectionViewGroup))
            //     return base.CreateContainer(item);
            //
            // var container = new GroupItem()
            // {
            //     ViewGroup = collectionViewGroup
            // };
            // return container;
        }
        
    }
}
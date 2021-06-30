using System.Xml.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Mvolonia.Controls.Collections;

namespace Mvolonia.Controls.Generators
{
    public class GroupItemContainerGenerator : ItemContainerGenerator<ListBoxItem>, IGroupableItemContainerGenerator
    {
        public GroupItemContainerGenerator(IControl owner, AvaloniaProperty contentProperty, AvaloniaProperty contentTemplateProperty) : base(owner, contentProperty, contentTemplateProperty)
        {
        }
        
        public GroupStyle GroupStyle { get; set; }
            
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
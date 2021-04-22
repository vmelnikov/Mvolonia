using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Mvolonia.Controls
{
    public class GroupItem : TemplatedControl
    {
        private Control _header;
        
        public static readonly DirectProperty<ItemsControl, IEnumerable> ItemsProperty =
            AvaloniaProperty.RegisterDirect<ItemsControl, IEnumerable>(nameof(Items), o => o.Items, (o, v) => o.Items = v);

        private IEnumerable _items;

        public IEnumerable Items
        {
            get =>  _items;
            set => SetAndRaise(ItemsProperty, ref _items, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _header =  e.NameScope.Find<Control>("PART_Header");
            base.OnApplyTemplate(e);
        }
    }
}
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Mvolonia.Controls.Collections;
using Mvolonia.Controls.Generators;

namespace Mvolonia.Controls
{
    
    public class GroupItem : TemplatedControl, IItemsPresenterHost
    {
        private Control _header;
        
        private IEnumerable _items;
        private IItemContainerGenerator _itemContainerGenerator;
        private CollectionViewGroup _viewGroup;
        private StackPanel _panel;

        /// <summary>
        /// Gets or sets the data template used to display the items in the control.
        /// </summary>
        public IDataTemplate ItemTemplate { get; private set; }
        

        public StackPanel Panel => _panel;
        
        /// <summary>
        /// Gets the items presenter control.
        /// </summary>
        public IItemsPresenter Presenter
        {
            get;
            protected set;
        }
        
        /// <summary>
        /// Gets the <see cref="IItemContainerGenerator"/> for the control.
        /// </summary>
        public IItemContainerGenerator ItemContainerGenerator
        {
            get
            {
                if (_itemContainerGenerator != null) 
                    return _itemContainerGenerator;
                _itemContainerGenerator = CreateItemContainerGenerator();

                if (_itemContainerGenerator is null) 
                    return null;
                
                _itemContainerGenerator.ItemTemplate = ItemTemplate;
                _itemContainerGenerator.Materialized += (_, e) => OnContainersMaterialized(e);
                _itemContainerGenerator.Dematerialized += (_, e) => OnContainersDematerialized(e);
                _itemContainerGenerator.Recycled += (_, e) => OnContainersRecycled(e);

                return _itemContainerGenerator;
            }
        }

        internal CollectionViewGroup ViewGroup
        {
            get => _viewGroup;
            set
            {
                if (_viewGroup == value)
                    return;
                _viewGroup = value;
                _items = ViewGroup.Items;
                UpdateHeader();
            }
        }

        private void OnContainersRecycled(ItemContainerEventArgs itemContainerEventArgs)
        {
            var groupingListBox = GetParent<GroupingListBox>(); 
            groupingListBox?.ChildContainersRecycled(itemContainerEventArgs);
        }

        private void OnContainersDematerialized(ItemContainerEventArgs itemContainerEventArgs)
        {
            var groupingListBox = GetParent<GroupingListBox>(); 
            groupingListBox?.ChildContainersDematerialized(itemContainerEventArgs);
        }

        private void OnContainersMaterialized(ItemContainerEventArgs itemContainerEventArgs)
        {
            var groupingListBox = GetParent<GroupingListBox>(); 
            groupingListBox?.ChildContainersMaterialized(itemContainerEventArgs);
        }

        private T GetParent<T>() where T: class
        {
            IControl control = this;
            while (!(control.Parent is null))
            {
                control = control.Parent;
                if (control is T t)
                    return t;
            }

            return null;
        }

        private IItemContainerGenerator CreateItemContainerGenerator()
        {
            return new GroupItemContainerGenerator(
                this, 
                ListBoxItem.ContentProperty,
                ListBoxItem.ContentTemplateProperty);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _header =  e.NameScope.Find<Control>("PART_Header");
            _panel =  e.NameScope.Find<StackPanel>("PART_Panel");
            UpdateHeader();
            base.OnApplyTemplate(e);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            var itemsControl = GetParent<ItemsControl>(); 
            if (!(itemsControl is null))
                ItemTemplate = itemsControl.ItemTemplate;
            base.OnAttachedToVisualTree(e);
        }

        private void UpdateHeader()
        {
            if (! (_header is ContentPresenter contentPresenter))
                return;
            if (ViewGroup is null)
                return;
            var groupingListBox = GetParent<GroupingListBox>();
            if (groupingListBox is null)
            {
                contentPresenter.Content = new TextBlock()
                {
                    Text = ViewGroup.Key.ToString()
                };
                return;
            }

            contentPresenter.Content = groupingListBox.GroupStyle[0].HeaderTemplate.Build(ViewGroup);
            contentPresenter.DataContext = ViewGroup;
        }

        public void RegisterItemsPresenter(IItemsPresenter presenter)
        {
            Presenter = presenter;
        }
    }
}
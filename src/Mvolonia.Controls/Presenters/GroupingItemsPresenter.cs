using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Mvolonia.Controls.Presenters
{
    public class GroupingItemsPresenter : ItemsPresenterBase, IItemContainerGeneratorHolder, ILogicalScrollable
    {
        private const double ScrollViewerDefaultSmallChange = 16;
        
        /// <summary>
        /// Defines the <see cref="VirtualizationMode"/> property.
        /// </summary>
        public static readonly StyledProperty<ItemVirtualizationMode> VirtualizationModeProperty =
            AvaloniaProperty.Register<ItemsPresenter, ItemVirtualizationMode>(
                nameof(VirtualizationMode),
                defaultValue: ItemVirtualizationMode.None);

        private bool _canHorizontallyScroll;
        private bool _canVerticallyScroll;
        private EventHandler _scrollInvalidated;

        /// <summary>
        /// Initializes static members of the <see cref="ItemsPresenter"/> class.
        /// </summary>
        static GroupingItemsPresenter()
        {
            KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue(
                typeof(GroupingItemsPresenter),
                KeyboardNavigationMode.Once);

            VirtualizationModeProperty.Changed
                .AddClassHandler<GroupingItemsPresenter>((x, e) => x.VirtualizationModeChanged(e));
        }

        /// <summary>
        /// Gets or sets the virtualization mode for the items.
        /// </summary>
        public ItemVirtualizationMode VirtualizationMode
        {
            get => GetValue(VirtualizationModeProperty);
            set => SetValue(VirtualizationModeProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the content can be scrolled horizontally.
        /// </summary>
        bool ILogicalScrollable.CanHorizontallyScroll
        {
            get => _canHorizontallyScroll;
            set
            {
                _canHorizontallyScroll = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the content can be scrolled horizontally.
        /// </summary>
        bool ILogicalScrollable.CanVerticallyScroll
        {
            get => _canVerticallyScroll;
            set
            {
                _canVerticallyScroll = value;
                InvalidateMeasure();
            }
        }
        /// <inheritdoc/>
        bool ILogicalScrollable.IsLogicalScrollEnabled
        {
            get { return Virtualizer?.IsLogicalScrollEnabled ?? false; }
        }

        /// <inheritdoc/>
        Size IScrollable.Extent => Virtualizer?.Extent ?? Size.Empty;

        /// <inheritdoc/>
        Vector IScrollable.Offset
        {
            get { return Virtualizer?.Offset ?? new Vector(); }
            set
            {
                if (Virtualizer != null)
                {
                    Virtualizer.Offset = CoerceOffset(value);
                }
            }
        }

        /// <inheritdoc/>
        Size IScrollable.Viewport => Virtualizer?.Viewport ?? Bounds.Size;

        /// <inheritdoc/>
        event EventHandler ILogicalScrollable.ScrollInvalidated
        {
            add => _scrollInvalidated += value;
            remove => _scrollInvalidated -= value;
        }

        /// <inheritdoc/>
        Size ILogicalScrollable.ScrollSize => new Size(ScrollViewerDefaultSmallChange, 1);

        /// <inheritdoc/>
        Size ILogicalScrollable.PageScrollSize => Virtualizer?.Viewport ?? new Size(16, 16);

        internal IItemVirtualizer Virtualizer { get; private set; }

        /// <inheritdoc/>
        bool ILogicalScrollable.BringIntoView(IControl target, Rect targetRect)
        {
            return false;
        }

        /// <inheritdoc/>
        IControl ILogicalScrollable.GetControlInDirection(NavigationDirection direction, IControl from)
        {
            return Virtualizer?.GetControlInDirection(direction, from);
        }

        /// <inheritdoc/>
        void ILogicalScrollable.RaiseScrollInvalidated(EventArgs e)
        {
            _scrollInvalidated?.Invoke(this, e);
        }

        public override void ScrollIntoView(int index)
        {
            Virtualizer?.ScrollIntoView(index);
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            return Virtualizer?.MeasureOverride(availableSize) ?? Size.Empty;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return Virtualizer?.ArrangeOverride(finalSize) ?? Size.Empty;
        }

        /// <inheritdoc/>
        protected override void PanelCreated(IPanel panel)
        {
            Virtualizer?.Dispose();
            Virtualizer = ItemVirtualizerBase.Create(this, VirtualizationMode);
            _scrollInvalidated?.Invoke(this, EventArgs.Empty);

            KeyboardNavigation.SetTabNavigation(
                (InputElement)Panel,
                KeyboardNavigation.GetTabNavigation(this));
        }

        protected override void ItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            Virtualizer?.ItemsChanged(Items, e);
        }

        private Vector CoerceOffset(Vector value)
        {
            var scrollable = (ILogicalScrollable)this;
            var maxX = Math.Max(scrollable.Extent.Width - scrollable.Viewport.Width, 0);
            var maxY = Math.Max(scrollable.Extent.Height - scrollable.Viewport.Height, 0);
            return new Vector(Math.Max(Math.Min(value.X, maxX), 0), Math.Max(Math.Min(value.Y, maxY), 0));
        }

        private void VirtualizationModeChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Virtualizer?.Dispose();
            Virtualizer = ItemVirtualizerBase.Create(this, VirtualizationMode);
            _scrollInvalidated?.Invoke(this, EventArgs.Empty);
        }
    
        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            var i = TemplatedParent as ItemsControl;
            var result = i?.ItemContainerGenerator;

            if (result is null)
            {
                result = new ItemContainerGenerator(this) {ItemTemplate = ItemTemplate};
            }

            return result;
        }
    }
}
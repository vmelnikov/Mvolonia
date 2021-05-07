using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Mvolonia.Controls.Collections;
using Mvolonia.Controls.Utils;

namespace Mvolonia.Controls.Presenters
{
    public abstract class ItemVirtualizerBase: IItemVirtualizer
    {
        private double _crossAxisOffset;
        private IDisposable _subscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemVirtualizerBase"/> class.
        /// </summary>
        /// <param name="owner"></param>
        protected ItemVirtualizerBase(IItemsPresenter owner)
        {
            Owner = owner;
            UpdateItems(owner.Items);

            var panel = VirtualizingPanel;

            if (panel != null)
            {
                _subscriptions = panel.GetObservable(Panel.BoundsProperty)
                    .Skip(1)
                    .Subscribe(_ => InvalidateScroll());
            }
        }
        

        /// <summary>
        /// Gets the <see cref="ItemsPresenter"/> which owns the virtualizer.
        /// </summary>
        public IItemsPresenter Owner { get; }

        /// <summary>
        /// Gets the <see cref="IItemContainerGenerator"/> from Owner
        /// </summary>
        protected IItemContainerGenerator ItemContainerGenerator => 
            Owner.GetItemContainerGenerator();

        /// <summary>
        /// Gets the <see cref="IVirtualizingPanel"/> which will host the items.
        /// </summary>
        public IVirtualizingPanel VirtualizingPanel => Owner.Panel as IVirtualizingPanel;

        /// <summary>
        /// Gets the items to display.
        /// </summary>
        public IEnumerable Items { get; private set; }

        /// <summary>
        /// Gets the number of items in <see cref="Items"/>.
        /// </summary>
        public int ItemCount { get; private set; }

        /// <summary>
        /// Gets or sets the index of the first item displayed in the panel.
        /// </summary>
        public int FirstIndex { get; protected set; }

        /// <summary>
        /// Gets or sets the index of the first item beyond those displayed in the panel.
        /// </summary>
        public int NextIndex { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the items should be scroll horizontally or vertically.
        /// </summary>
        public bool Vertical => VirtualizingPanel?.ScrollDirection == Orientation.Vertical;

        /// <summary>
        /// Gets a value indicating whether logical scrolling is enabled.
        /// </summary>
        public abstract bool IsLogicalScrollEnabled { get; }

        /// <summary>
        /// Gets the value of the scroll extent.
        /// </summary>
        public abstract double ExtentValue { get; }

        /// <summary>
        /// Gets or sets the value of the current scroll offset.
        /// </summary>
        public abstract double OffsetValue { get; set; }

        /// <summary>
        /// Gets the value of the scrollable viewport.
        /// </summary>
        public abstract double ViewportValue { get; }

        /// <summary>
        /// Gets the <see cref="ExtentValue"/> as a <see cref="Size"/>.
        /// </summary>
        public Size Extent
        {
            get
            {
                if (IsLogicalScrollEnabled)
                {
                    return Vertical ?
                        new Size(Owner.Panel.DesiredSize.Width, ExtentValue) :
                        new Size(ExtentValue, Owner.Panel.DesiredSize.Height);
                }

                return default;
            }
        }

        /// <summary>
        /// Gets the <see cref="ViewportValue"/> as a <see cref="Size"/>.
        /// </summary>
        public Size Viewport
        {
            get
            {
                if (IsLogicalScrollEnabled)
                {
                    return Vertical ?
                        new Size(Owner.Panel.Bounds.Width, ViewportValue) :
                        new Size(ViewportValue, Owner.Panel.Bounds.Height);
                }

                return default;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="OffsetValue"/> as a <see cref="Vector"/>.
        /// </summary>
        public Vector Offset
        {
            get
            {
                if (IsLogicalScrollEnabled)
                {
                    return Vertical ? new Vector(_crossAxisOffset, OffsetValue) : new Vector(OffsetValue, _crossAxisOffset);
                }

                return default;
            }

            set
            {
                if (!IsLogicalScrollEnabled)
                {
                    throw new NotSupportedException("Logical scrolling disabled.");
                }

                var oldCrossAxisOffset = _crossAxisOffset;

                if (Vertical)
                {
                    OffsetValue = value.Y;
                    _crossAxisOffset = value.X;
                }
                else
                {
                    OffsetValue = value.X;
                    _crossAxisOffset = value.Y;
                }

                if (_crossAxisOffset != oldCrossAxisOffset)
                {
                    Owner.InvalidateArrange();
                }
            }
        }
        
        /// <summary>
        /// Creates an <see cref="ItemVirtualizer"/> based on an item presenter's 
        /// <see cref="ItemVirtualizationMode"/>.
        /// </summary>
        /// <param name="owner">The items presenter.</param>
        /// <returns>An <see cref="ItemVirtualizer"/>.</returns>
        public static IItemVirtualizer Create(IItemsPresenter owner, ItemVirtualizationMode virtualizationMode)
        {
            if (owner.Panel == null)
            {
                return null;
            }
        
            var virtualizingPanel = owner.Panel as IVirtualizingPanel;
            var scrollContentPresenter = owner.Parent as IScrollable;
            IItemVirtualizer result = null;
        
            if (virtualizingPanel != null && scrollContentPresenter != null)
            {
                switch (virtualizationMode)
                {
                    case ItemVirtualizationMode.Simple:
                        result = new GroupingItemVirtualizerSimple(owner);
                        break;
                }
            }
        
            if (result == null)
            {
                return new GroupingItemsVirtualizerNone(owner);
            }
        
            if (virtualizingPanel != null)
            {
                virtualizingPanel.Controller = result;
            }
        
            return result;
        }

        /// <summary>
        /// Carries out a measure for the related <see cref="ItemsPresenter"/>.
        /// </summary>
        /// <param name="availableSize">The size available to the control.</param>
        /// <returns>The desired size for the control.</returns>
        public virtual Size MeasureOverride(Size availableSize)
        {
            Owner.Panel.Measure(availableSize);
            return Owner.Panel.DesiredSize;
        }

        /// <summary>
        /// Carries out an arrange for the related <see cref="ItemsPresenter"/>.
        /// </summary>
        /// <param name="finalSize">The size available to the control.</param>
        /// <returns>The actual size used.</returns>
        public virtual Size ArrangeOverride(Size finalSize)
        {
            if (VirtualizingPanel != null)
            {
                VirtualizingPanel.CrossAxisOffset = _crossAxisOffset;
                Owner.Panel.Arrange(new Rect(finalSize));
            }
            else
            {
                var origin = Vertical ? new Point(-_crossAxisOffset, 0) : new Point(0, _crossAxisOffset);
                Owner.Panel.Arrange(new Rect(origin, finalSize));
            }

            return finalSize;
        }

        /// <inheritdoc/>
        public virtual void UpdateControls()
        {
        }
        
        private void UpdateItems(IEnumerable items)
        {
            // if (items is ICollectionView collectionView &&
            //     collectionView.IsGrouping)
            // {
            //     Items = collectionView.GroupingItems;
            //     Count = collectionView.GroupingItems.Count();
            //     return;
            // }
            Items = items;
            ItemCount = items.Count();
        }

        /// <summary>
        /// Gets the next control in the specified direction.
        /// </summary>
        /// <param name="direction">The movement direction.</param>
        /// <param name="from">The control from which movement begins.</param>
        /// <returns>The control.</returns>
        public virtual IControl GetControlInDirection(NavigationDirection direction, IControl from)
        {
            return null;
        }

        /// <summary>
        /// Called when the items for the presenter change, either because 
        /// <see cref="ItemsPresenterBase.Items"/> has been set, the items collection has been
        /// modified, or the panel has been created.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="e">A description of the change.</param>
        public virtual void ItemsChanged(IEnumerable items, NotifyCollectionChangedEventArgs e) =>
            UpdateItems(items);

        /// <summary>
        /// Scrolls the specified item into view.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        public virtual void ScrollIntoView(int index)
        {
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            if (VirtualizingPanel != null)
            {
                VirtualizingPanel.Controller = null;
                VirtualizingPanel.Children.Clear();
            }

            ItemContainerGenerator?.Clear();
        }

        /// <summary>
        /// Invalidates the current scroll.
        /// </summary>
        protected void InvalidateScroll() => ((ILogicalScrollable)Owner).RaiseScrollInvalidated(EventArgs.Empty);
    }
}
using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Mvolonia.Controls.Presenters
{
    public interface IItemVirtualizer : IVirtualizingController, IDisposable
    {
        
        bool IsLogicalScrollEnabled { get; }
        
        Size Extent { get; }
        
        Vector Offset { get; set; }
        
        Size Viewport { get; }

        Size ArrangeOverride(Size finalSize);

        IControl GetControlInDirection(NavigationDirection direction, IControl from);
        
        void ScrollIntoView(int index);
        
        Size MeasureOverride(Size availableSize);

        void ItemsChanged(IEnumerable items, NotifyCollectionChangedEventArgs e);
    }
}
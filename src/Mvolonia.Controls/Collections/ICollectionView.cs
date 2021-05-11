using System.Collections;
using System.Collections.Specialized;

namespace Mvolonia.Controls.Collections
{
    
    internal interface ICollectionView : IList, INotifyCollectionChanged
    {
        bool IsGrouping { get; }
        
        CollectionViewGroup FindGroupContainingItem(object item);
        
    }
}
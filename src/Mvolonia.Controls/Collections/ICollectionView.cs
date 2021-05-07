using System.Collections;
using System.Collections.Specialized;

namespace Mvolonia.Controls.Collections
{
    
    public interface ICollectionView : IList, INotifyCollectionChanged
    {
        bool IsGrouping { get; }
        
        IEnumerable GroupingItems { get; }
    }
}
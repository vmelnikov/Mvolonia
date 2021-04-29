using System.Collections;

namespace Mvolonia.Controls.Collections
{
    
    public interface ICollectionView : IEnumerable
    {
        bool IsGrouping { get; }
        
        IEnumerable GroupingItems { get; }
    }
}
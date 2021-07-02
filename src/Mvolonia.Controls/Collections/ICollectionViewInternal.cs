using System.Collections;
using System.Collections.Specialized;

namespace Mvolonia.Controls.Collections
{
    internal interface ICollectionViewInternal : ICollectionView
    {
        IEnumerable SourceCollection { get; }
        
        NotifyCollectionChangedEventArgs CurrentSourceCollectionChangedArgs { get; }
    }
}
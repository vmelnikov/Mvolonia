using System.Collections;
using System.Collections.Specialized;

namespace Mvolonia.Controls.Collections
{
    
    internal interface ICollectionView : IList, INotifyCollectionChanged
    {
        bool IsGrouping { get; }
        
        CollectionViewGroupRoot RootGroup { get; }
        
        /// <summary>Gets a collection of <see cref="T:Mvolonia.Controls.Collections.SortDescription" /> instances that describe how the items in the collection are sorted in the view.</summary>
        /// <returns>A collection of values that describe how the items in the collection are sorted in the view.</returns>
        SortDescriptionCollection SortDescriptions { get; }
        
        CollectionViewGroup FindGroupContainingItem(object item);

        bool ContainsGroup(CollectionViewGroup group);
    }
}
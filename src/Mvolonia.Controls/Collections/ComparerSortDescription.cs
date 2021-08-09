using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mvolonia.Controls.Collections
{
    public class ComparerSortDescription : SortDescription
    {
        private readonly IComparer _innerComparer;

        public ComparerSortDescription(IComparer comparer, ListSortDirection direction) : base(direction)
        {
            _innerComparer = comparer;
            Comparer = Comparer<object>.Create(Compare);
        }

        public override IComparer<object> Comparer { get; }
        
        
        private int Compare(object x, object y)
        {
            var result = _innerComparer.Compare(x, y);

            if (Direction == ListSortDirection.Descending)
                return -result;
            return result;
        }

    }
}
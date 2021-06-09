using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mvolonia.Controls.Collections.Comparers
{
    internal class MergedComparer
    {
        private readonly IComparer<object>[] _comparers;

        private MergedComparer(SortDescriptionCollection coll)
        {
            _comparers = MakeComparerArray(coll);
        }

        public MergedComparer(CollectionView collectionView)
            : this(collectionView.SortDescriptions)
        {
        }

        private static IComparer<object>[] MakeComparerArray(SortDescriptionCollection coll) =>
            coll.Select(c => c.Comparer).ToArray();
        

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to or greater than the other.
        /// </summary>
        /// <param name="x">first item to compare</param>
        /// <param name="y">second item to compare</param>
        /// <returns>Negative number if x is less than y, zero if equal, and a positive number if x is greater than y</returns>
        /// <remarks>
        /// Compares the 2 items using the list of property names and directions.
        /// </remarks>
        public int Compare(object x, object y)
        {
            var result = 0;

            // compare both objects by each of the properties until property values don't match
            foreach (var comparer in _comparers)
            {
                result = comparer.Compare(x, y);
                if (result != 0)
                    return result;
            }

            return result;
        }

        /// <summary>
        /// Steps through the given list using the comparer to find where
        /// to insert the specified item to maintain sorted order
        /// </summary>
        /// <param name="x">Item to insert into the list</param>
        /// <param name="list">List where we want to insert the item</param>
        /// <returns>Index where we should insert into</returns>
        public int FindInsertIndex(object x, IList list)
        {
            var min = 0;
            var max = list.Count - 1;

            // run a binary search to find the right index
            // to insert into.
            while (min <= max)
            {
                var index = (min + max) / 2;

                var result = Compare(x, list[index]);
                if (result == 0)
                    return index;
                
                if (result > 0)
                    min = index + 1;
                else
                    max = index - 1;
            }
            return min;
        }
    }
}
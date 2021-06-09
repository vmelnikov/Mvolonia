using System.Collections;

namespace Mvolonia.Controls.Collections.Comparers
{
    //  <summary>
    //  This comparer is used to insert an item into a group in a position consistent
    //  with a given IList.  It only works when used in the pattern that FindIndex
    //  uses, namely first call Reset(), then call Compare(item, itemSequence) any number of
    //  times with the same item (the new item) as the first argument, and a sequence
    //  of items as the second argument that appear in the IList in the same sequence.
    //  This makes the total search time linear in the size of the IList.  (To give
    //  the correct answer regardless of the sequence of arguments would involve
    //  calling IndexOf and leads to O(N^2) total search time.) 
    //  </summary>
    internal class ListComparer : IComparer
    {
        /// <summary>
        /// Constructor for the ListComparer that takes
        /// in an IList.
        /// </summary>
        /// <param name="list">IList used to compare on</param>
        internal ListComparer(IList list)
        {
            ResetList(list);
        }

        /// <summary>
        /// Sets the index that we start comparing
        /// from to 0.
        /// </summary>
        internal void Reset()
        {
            _index = 0;
        }

        /// <summary>
        /// Sets our IList to a new instance
        /// of a list being passed in and resets
        /// the index.
        /// </summary>
        /// <param name="list">IList used to compare on</param>
        internal void ResetList(IList list)
        {
            _list = list;
            _index = 0;
        }

        /// <summary>
        /// Compares objects x and y to see which one
        /// should appear first.
        /// </summary>
        /// <param name="x">The first object</param>
        /// <param name="y">The second object</param>
        /// <returns>-1 if x is less than y, +1 otherwise</returns>
        public int Compare(object x, object y)
        {
            if (Equals(x, y))
                return 0;

            if (_list is null)
                return 0;

            // advance the index until seeing one x or y
            var n = _list.Count;
            for (; _index < n; ++_index)
            {
                var z = _list[_index];
                if (Equals(x, z))
                    return -1; // x occurs first, so x < y

                if (Equals(y, z))
                    return +1; // y occurs first, so x > y
            }

            // if we don't see either x or y, declare x > y.
            // This has the effect of putting x at the end of the list.
            return +1;
        }

        private int _index;
        private IList _list;
    }
}

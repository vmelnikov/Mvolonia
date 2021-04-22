using System;
using System.Collections;
using System.Diagnostics;

namespace Mvolonia.Controls.Collections
{
    /// <summary>
    /// Enumerator for the leaves in the CollectionViewGroupInternal class.
    /// </summary>
    internal class LeafEnumerator : IEnumerator
    {
        private object _current;   // current item
        private readonly CollectionViewGroupInternal _group;
        private int _index;     // current index into Items
        private IEnumerator _subEnum;   // enumerator over current subgroup

        public LeafEnumerator(CollectionViewGroupInternal group)
        {
            _group = group;
            
            DoReset();  // don't call virtual Reset in ctor
        }

        object IEnumerator.Current
        {
            get
            {
                Debug.Assert(_group != null, "_group should have been initialized in constructor");

                if (_index < 0 || _index >= _group.Items.Count)
                {
                    throw new InvalidOperationException();
                }

                return _current;
            }
        }

        private void DoReset()
        {
            _index = -1;
            _subEnum = null;
        }


        public bool MoveNext()
        {
            // move forward to the next leaf
            while (_subEnum == null || !_subEnum.MoveNext())
            {
                // done with the current top-level item.  Move to the next one.
                ++_index;
                if (_index >= _group.Items.Count)
                {
                    return false;
                }

                if (_group.Items[_index] is CollectionViewGroupInternal subGroup)
                {
                    // current item is a subgroup - get its enumerator
                    _subEnum = subGroup.GetLeafEnumerator();
                }
                else
                {
                    // current item is a leaf - it's the new Current
                    _current = _group.Items[_index];
                    _subEnum = null;
                    return true;
                }
            }

            // the loop terminates only when we have a subgroup enumerator
            // positioned at the new Current item
            _current = _subEnum.Current;
            return true;
        }

        public void Reset() =>
            DoReset();
    }
}
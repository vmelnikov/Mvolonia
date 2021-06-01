using System;
using System.ComponentModel;

namespace Mvolonia.Controls.Collections
{
    /// <summary>
    /// Defines a property and direction to sort a list by.
    /// </summary>
    public abstract class SortDescription
    {
        private ListSortDirection _direction;

        public SortDescription(ListSortDirection direction)
        {
            if (direction != ListSortDirection.Ascending && direction != ListSortDirection.Descending)
                throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(ListSortDirection));
            _direction = direction;
            IsSealed = false;
        }
        
        
        /// <summary>
        /// Sort direction.
        /// </summary>
        public ListSortDirection Direction
        {
            get => _direction;
            set
            {
                if (IsSealed)
                    throw new InvalidOperationException("Can't change after sealed ");

                if (value < ListSortDirection.Ascending || value > ListSortDirection.Descending)
                    throw new InvalidEnumArgumentException(nameof(value), (int) value, typeof(ListSortDirection));

                _direction = value;
            }
        }
        
        /// <summary>
        /// Returns true if the SortDescription is in use (sealed).
        /// </summary>
        public bool IsSealed { get; private set; }


        internal void Seal() =>
            IsSealed = true;
        
    }
}
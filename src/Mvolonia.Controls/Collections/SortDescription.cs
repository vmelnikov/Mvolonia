using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Mvolonia.Controls.Collections
{
    /// <summary>
    /// Defines a property and direction to sort a list by.
    /// </summary>
    public abstract class SortDescription
    {
        public static PropertySortDescription FromPropertyName(string propertyName,
            ListSortDirection direction = ListSortDirection.Ascending, CultureInfo culture = null) =>
            new PropertySortDescription(propertyName, direction, culture);

        public static SortDescription FromComparer(IComparer comparer,
            ListSortDirection direction = ListSortDirection.Ascending) =>
            new ComparerSortDescription(comparer, direction);


        private ListSortDirection _direction;

        protected SortDescription(ListSortDirection direction)
        {
            if (direction != ListSortDirection.Ascending && direction != ListSortDirection.Descending)
                throw new InvalidEnumArgumentException(nameof(direction), (int) direction, typeof(ListSortDirection));
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
                    throw new InvalidOperationException("Can't change after seal");

                if (value < ListSortDirection.Ascending || value > ListSortDirection.Descending)
                    throw new InvalidEnumArgumentException(nameof(value), (int) value, typeof(ListSortDirection));

                _direction = value;
            }
        }

        public abstract IComparer<object> Comparer { get; }

        /// <summary>
        /// Returns true if the SortDescription is in use (sealed).
        /// </summary>
        public bool IsSealed { get; private set; }


        internal void Seal() =>
            IsSealed = true;

        public IEnumerable<object> OrderBy(IEnumerable<object> seq) =>
            seq.OrderBy(o => o, Comparer);

        public IEnumerable<object> ThenBy(IOrderedEnumerable<object> seq) =>
            seq.ThenBy(o => o, Comparer);
    }
}
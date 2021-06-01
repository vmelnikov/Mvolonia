using System;
using System.ComponentModel;

namespace Mvolonia.Controls.Collections
{
    /// <summary>
    /// Defines a property and direction to sort a list by.
    /// </summary>
    public class PropertySortDescription : SortDescription
    {
        private string _propertyName;

        public PropertySortDescription(string propertyName, ListSortDirection direction) : base(direction)
        {
            _propertyName = propertyName;
        }
        
        /// <summary>
        /// Property name to sort by.
        /// </summary>
        public string PropertyName
        {
            get => _propertyName;
            set
            {
                if (IsSealed)
                    throw new InvalidOperationException("Can't change after seal");

                _propertyName = value;
            }
        }
    }
}
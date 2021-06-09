using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Mvolonia.Controls.Collections.Comparers;
using Mvolonia.Controls.Utils;

namespace Mvolonia.Controls.Collections
{
    /// <summary>
    /// Defines a property and direction to sort a list by.
    /// </summary>
    public class PropertySortDescription : SortDescription
    {
        private static object InvokeName(object item, string propertyName, Type propertyType)
        {
            var propertyValue = TypeHelper.GetNestedPropertyValue(item, propertyName, propertyType, out Exception exception);
            if (exception != null)
            {
                throw exception;
            }
            return propertyValue;
        }
        
        
        private string _propertyName;
        private Type _propertyType;
        
        private readonly Lazy<IComparer<object>> _lazyComparer;
        private readonly Lazy<CultureSensitiveComparer> _lazyCultureSensitiveComparer;
        private IComparer _internalComparer;

        public PropertySortDescription(string propertyName, ListSortDirection direction, CultureInfo culture) : base(direction)
        {
            _propertyName = propertyName;
            _lazyComparer = new Lazy<IComparer<object>>(() => Comparer<object>.Create(Compare));
            _lazyCultureSensitiveComparer = new Lazy<CultureSensitiveComparer>(() => new CultureSensitiveComparer(culture ?? CultureInfo.CurrentCulture));
        }
        
        private bool HasPropertyName => !string.IsNullOrEmpty(PropertyName);
        
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

        public override IComparer<object> Comparer => _lazyComparer.Value;
        
        
        private object GetValue(object o)
        {
            if (o == null)
                return null;

            if (HasPropertyName)
                return InvokeName(o, _propertyName, _propertyType);

            return _propertyType == o.GetType() ? o : null;
        }
        
        private IComparer GetComparerForType(Type type) =>
            type == typeof(string) 
                ? _lazyCultureSensitiveComparer.Value
                : (typeof(Comparer<>).MakeGenericType(type).GetProperty("Default"))?.GetValue(null, null) as IComparer;
        
        private Type GetPropertyType(object o) =>
            o.GetType().GetNestedPropertyType(_propertyName);
        
        
        private int Compare(object x, object y)
        {
            UpdatePropertyType(x, y);
            UpdateInternalComparer();

            var v1 = GetValue(x);
            var v2 = GetValue(y);

            var result = _internalComparer?.Compare(v1, v2) ?? 0;

            if (Direction == ListSortDirection.Descending)
                return -result;
            return result;
        }

        private void UpdateInternalComparer()
        {
            if (!(_internalComparer is null))
                return;
            if (_propertyType is null)
                return;
            
            _internalComparer = GetComparerForType(_propertyType);
        }

        private void UpdatePropertyType(object x, object y)
        {
            if (!(_propertyType is null)) 
                return;
            
            if (!(x is null))
                _propertyType = GetPropertyType(x);

            if (_propertyType is null && !(y is null))
                _propertyType = GetPropertyType(y);
        }
    }
}
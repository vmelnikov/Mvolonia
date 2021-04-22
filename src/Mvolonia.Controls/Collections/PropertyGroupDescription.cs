using System;
using System.Globalization;
using Mvolonia.Controls.Utils;

namespace Mvolonia.Controls.Collections
{
    public class PropertyGroupDescription : GroupDescription
    {
        private readonly string _propertyName;
        private Type _propertyType;

        /// <summary>
        /// Initializes a new instance of PropertyGroupDescription.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property whose value is used to determine which group(s)
        /// an item belongs to.
        /// If PropertyName is null, the item itself is used.
        /// </param>
        public PropertyGroupDescription(string propertyName)
        {
             _propertyName = propertyName;
        }

        public override object GroupKeyFromItem(object item, int level, CultureInfo culture)
        {
            return GetKey(item) ?? item;
        }

        private object GetKey(object o)
        {
            if(o == null)
                return null;

            if (_propertyType == null)
                _propertyType = GetPropertyType(o);

            return InvokePath(o, _propertyName, _propertyType);
        }
        
        private Type GetPropertyType(object o)
        {
            return o.GetType().GetNestedPropertyType(_propertyName);
        }
        private static object InvokePath(object item, string propertyPath, Type propertyType)
        {
            var propertyValue = TypeHelper.GetNestedPropertyValue(item, propertyPath, propertyType, out var exception);
            if (exception != null)
            {
                throw exception;
            }
            return propertyValue;
        }
    }
}
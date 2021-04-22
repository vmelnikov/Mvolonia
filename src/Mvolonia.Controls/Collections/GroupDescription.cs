using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Collections;

namespace Mvolonia.Controls.Collections
{
    /// <summary>
    /// Base class for group descriptions.
    /// A GroupDescription describes how to divide the items in a collection
    /// into groups.
    /// </summary>
    public abstract class GroupDescription : INotifyPropertyChanged
    {
        public AvaloniaList<object> GroupKeys { get; }

        protected GroupDescription()
        {
            GroupKeys = new AvaloniaList<object>();
            GroupKeys.CollectionChanged += (sender, e) => OnPropertyChanged(new PropertyChangedEventArgs(nameof(GroupKeys)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public virtual string PropertyName => string.Empty;
        
        public abstract object GroupKeyFromItem(object item, int level, CultureInfo culture);
        
        public virtual bool KeysMatch(object groupKey, object itemKey)
        {
            return object.Equals(groupKey, itemKey);
        }
    }
}
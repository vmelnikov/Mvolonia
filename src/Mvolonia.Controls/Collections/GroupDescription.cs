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
        /// <summary>
        /// Names of explicit groups
        /// </summary>
        public AvaloniaList<object> GroupNames { get; }

        protected GroupDescription()
        {
            GroupNames = new AvaloniaList<object>();
            GroupNames.CollectionChanged += (sender, e) => OnPropertyChanged(new PropertyChangedEventArgs(nameof(GroupNames)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public virtual string PropertyName => string.Empty;
        
        public abstract object GroupKeyFromItem(object item, int level);
        
        public virtual bool KeysMatch(object groupKey, object itemKey)
        {
            return object.Equals(groupKey, itemKey);
        }
    }
}
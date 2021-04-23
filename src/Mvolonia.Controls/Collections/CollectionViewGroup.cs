using System.ComponentModel;
using Avalonia.Collections;

namespace Mvolonia.Controls.Collections
{
    /// <summary>
    /// A CollectionViewGroup, as created by a CollectionView according to a GroupDescription.
    /// </summary>
    internal abstract class CollectionViewGroup: INotifyPropertyChanged
    {
        protected CollectionViewGroup(object key)
        {
            Key = key;
            ProtectedItems = new AvaloniaList<object>();
        }
        
        public object Key { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        

        public IAvaloniaReadOnlyList<object> Items => ProtectedItems;

        public int ItemCount { get; private set; }

        protected AvaloniaList<object> ProtectedItems { get; }


        protected int ProtectedItemCount
        {
            get => ItemCount; 
            set
            {
                ItemCount = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ItemCount)));
            }
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
using System.ComponentModel;
using Avalonia.Collections;

namespace Mvolonia.Controls.Collections
{
    /// <summary>
    /// A CollectionViewGroup, as created by a CollectionView according to a GroupDescription.
    /// </summary>
    public abstract class CollectionViewGroup: INotifyPropertyChanged
    {
        private readonly object _key;

        protected CollectionViewGroup(object key)
        {
            _key = key;
            ProtectedItems = new AvaloniaList<object>();
        }

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
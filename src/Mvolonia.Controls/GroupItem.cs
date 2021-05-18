using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Mvolonia.Controls.Collections;

namespace Mvolonia.Controls
{
    
    public class GroupItem : TemplatedControl, IGroupItem
    {
        private Control _header;
        private CollectionViewGroup _viewGroup;
        
        /// <inheritdoc/>
        public bool IsEmpty => Panel?.Children?.Count == 0;

        public IPanel Panel { get; private set; }
        
        internal CollectionViewGroup ViewGroup
        {
            get => _viewGroup;
            set
            {
                if (_viewGroup == value)
                    return;
                _viewGroup = value;
                UpdateHeader();
            }
        }
        

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _header =  e.NameScope.Find<Control>("PART_Header");
            Panel =  e.NameScope.Find<StackPanel>("PART_Panel");
            UpdateHeader();
            base.OnApplyTemplate(e);
        }


        private void UpdateHeader()
        {
            if (! (_header is ContentPresenter contentPresenter))
                return;
            if (ViewGroup is null)
                return;
            var groupingListBox = GetParent<GroupingListBox>();
            if (groupingListBox is null)
            {
                contentPresenter.Content = new TextBlock()
                {
                    Text = ViewGroup.Key.ToString()
                };
                return;
            }

            contentPresenter.Content = groupingListBox.GroupStyle[0].HeaderTemplate.Build(ViewGroup);
            contentPresenter.DataContext = ViewGroup;
        }


        private T GetParent<T>() where T: class
        {
            IControl control = this;
            while (!(control.Parent is null))
            {
                control = control.Parent;
                if (control is T t)
                    return t;
            }

            return null;
        }
    }
}
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
        private int _level;

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
                _level = GetViewGroupLevel(value as CollectionViewGroupInternal);
                UpdateHeader();
            }
        }

        private static int GetViewGroupLevel(CollectionViewGroupInternal group)
        {
            var level = 0;
            while (true)
            {
                group = group.Parent;
                switch (@group)
                {
                    case null:
                        return level;
                    case CollectionViewGroupRoot _:
                        return level;
                    default:
                        level++;
                        break;
                }
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
            var host = GetParent<IGroupingItemsGeneratorHost>();
            var groupStyle = host?.GetGroupStyle(_level);
            contentPresenter.Content = BuildHeaderContent(groupStyle);
            contentPresenter.DataContext = ViewGroup;
        }

        private object BuildHeaderContent(GroupStyle groupStyle)
        {
            if (groupStyle is null)
                return new TextBlock()
                {
                    Text = ViewGroup.Key.ToString()
                };
            return groupStyle.HeaderTemplate.Build(ViewGroup);
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
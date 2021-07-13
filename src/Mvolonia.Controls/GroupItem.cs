using System;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Mvolonia.Controls.Collections;

namespace Mvolonia.Controls
{
    
    public class GroupItem : TemplatedControl, IGroupItem
    {
        private Control _header;
        private int _level;

        /// <inheritdoc/>
        public bool IsEmpty => Panel?.Children?.Count == 0;

        public IPanel Panel { get; private set; }


        internal void PrepareItemContainer(IGroupableItemsGeneratorHost host)
        {
            var groupStyle = host?.GetGroupStyle(_level);
            groupStyle?.ContainerStyle?.TryAttach(this, host as IStyleHost);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            _level = GetViewGroupLevel(DataContext as CollectionViewGroupInternal);
            UpdateHeader();
            base.OnDataContextChanged(e);
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
            if (_header is not ContentPresenter contentPresenter)
                return;
            if (DataContext is null)
                return;
            var host = GetParent<IGroupableItemsGeneratorHost>();
            var groupStyle = host?.GetGroupStyle(_level);
            contentPresenter.Content = BuildHeaderContent(groupStyle);
            contentPresenter.DataContext = DataContext;
        }

        private object BuildHeaderContent(GroupStyle groupStyle)
        {
            if (groupStyle is null)
                return new TextBlock()
                {
                    Text = (DataContext as CollectionViewGroup)?.Key.ToString()
                };
            return groupStyle.HeaderTemplate?.Build(DataContext);
        }


        private T GetParent<T>() where T: class
        {
            IControl control = this;
            while (control.Parent is not null)
            {
                control = control.Parent;
                if (control is T t)
                    return t;
            }

            return null;
        }
    }
}
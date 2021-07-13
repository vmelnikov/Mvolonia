using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Presenters;
using Mvolonia.Controls.Collections;
using Mvolonia.Controls.Utils;

namespace Mvolonia.Controls.Presenters
{
    internal static class ItemContainerSync
    {
        public static void ItemsChanged(
            IItemsPresenter owner,
            IEnumerable items,
            NotifyCollectionChangedEventArgs e)
        {
            var generator = owner.GetItemContainerGenerator();
            var panel = owner.Panel;

            if (panel == null)
            {
                return;
            }

            void Add()
            {
                if (e.NewStartingIndex + e.NewItems.Count < items.Count())
                {
                    generator.InsertSpace(e.NewStartingIndex, e.NewItems.Count);
                }

                AddContainers(owner, e.NewStartingIndex, e.NewItems);
            }

            void Remove()
            {
                RemoveContainers(owner, generator.RemoveRange(e.OldStartingIndex, e.OldItems.Count));
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add();
                    break;

                case NotifyCollectionChangedAction.Remove:
                    Remove();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    RemoveContainers(owner, generator.Dematerialize(e.OldStartingIndex, e.OldItems.Count));
                    var containers = AddContainers(owner, e.NewStartingIndex, e.NewItems);

                    var i = e.NewStartingIndex;

                    foreach (var container in containers)
                    {
                        panel.Children[i++] = container.ContainerControl;
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                    Remove();
                    Add();
                    break;

                case NotifyCollectionChangedAction.Reset:
                    RemoveContainers(owner, generator.Clear());

                    if (items != null)
                    {
                        AddContainers(owner, 0, items);
                    }

                    break;
            }
        }

        public static void FillExplicitGroupItems(IItemsPresenter owner)
        {
            var panel = owner.Panel;
            if (!(owner.Items is ICollectionView collectionView))
                return;
            if (!collectionView.IsGrouping)
                return;
            if (panel is null)
                return;
            AddGroupItemsToPanel(panel, 0, collectionView.RootGroup.Items);
        }

        public static IList<ItemContainerInfo> AddContainers(
            IItemsPresenter owner,
            int index,
            IEnumerable items)
        {
            var generator = owner.GetItemContainerGenerator();
            var panel = owner.Panel;

            if (owner.Items is ICollectionView collectionView &&
                collectionView.IsGrouping)
                return AddContainersToGroups(generator, panel, collectionView, index, items);


            var result = new List<ItemContainerInfo>();

            foreach (var item in items)
            {
                var materialized = generator.Materialize(index++, item);

                if (materialized.ContainerControl != null)
                {
                    if (materialized.Index < panel.Children.Count)
                        panel.Children.Insert(materialized.Index, materialized.ContainerControl);
                    else
                        panel.Children.Add(materialized.ContainerControl);
                }

                result.Add(materialized);
            }

            return result;
        }

        private static IList<ItemContainerInfo> AddContainersToGroups(IItemContainerGenerator generator, IPanel panel,
            ICollectionView collectionView, int index, IEnumerable items)
        {
            var result = new List<ItemContainerInfo>();

            foreach (var item in items)
            {
                var materialized = generator.Materialize(index++, item);

                AddContainerToGroupItem(panel, collectionView.FindGroupContainingItem(item), materialized);

                result.Add(materialized);
            }

            return result;
        }

        private static void AddGroupItemsToPanel(IPanel panel, int index, IEnumerable items)
        {
            if (panel is null)
                return;
            foreach (var item in items)
            {
                if (!(item is CollectionViewGroupInternal group))
                    continue;
                var groupItem = new GroupItem
                {
                    DataContext = group
                };
                
                panel.Children.Insert(index, groupItem);
                groupItem.PrepareItemContainer(GetParent<IGroupableItemsGeneratorHost>(panel));
                groupItem.ApplyTemplate();
                
                AddGroupItemsToPanel(groupItem.Panel, 0, group.Items);
                index++;
            }
        }

        private static void AddContainerToGroupItem(IPanel panel, CollectionViewGroup group,
            ItemContainerInfo container)
        {
            if (group is null)
                return;
            var containerPath = GetContainerPath(group, container);

            AddContainerPathToPanel(containerPath, panel);
        }

        private static void AddContainerPathToPanel(IList<object> containerPath, IPanel panel)
        {
            CollectionViewGroup group = null;
            for (var i = 0; i < containerPath.Count; i++)
            {
                var pathItem = containerPath[i];
                switch (pathItem)
                {
                    case CollectionViewGroup pathGroup:
                    {
                        var groupItem = GetOrAddGroupItem(pathGroup, panel);
                        panel = groupItem.Panel;
                        group = pathGroup;
                        break;
                    }
                    case ItemContainerInfo container:
                        AddContainerToPanel(panel, group, container);

                        break;
                }
            }
        }

        private static void AddContainerToPanel(IPanel panel, CollectionViewGroup group, ItemContainerInfo container)
        {
            if (group is null)
                throw new ArgumentNullException();
            if (panel is null)
                return;
            var index = group.Items.IndexOf(container.Item);
            if (index < 0 || index > panel.Children.Count)
                index = panel.Children.Count;
            panel.Children.Insert(index, container.ContainerControl);
        }

        private static GroupItem GetOrAddGroupItem(CollectionViewGroup group, IPanel panel)
        {
            if (!(group is CollectionViewGroupInternal groupInternal))
                throw new ArgumentException();
            var groupItem = panel.Children.OfType<GroupItem>()
                .FirstOrDefault(c => Equals(c.DataContext, group));

            if (!(groupItem is null))
                return groupItem;
            groupItem = new GroupItem
            {
                DataContext = group
            };
            groupItem.PrepareItemContainer(GetParent<IGroupableItemsGeneratorHost>(panel));
            var index = groupInternal.Parent?.Items.IndexOf(group) ?? -1;
            if (index < 0 || index > panel.Children.Count)
                index = panel.Children.Count;
            panel.Children.Insert(index, groupItem);
            groupItem.ApplyTemplate();
            AddGroupItemsToPanel(groupItem.Panel, 0, group.Items);
            return groupItem;
        }

        private static IList<object> GetContainerPath(CollectionViewGroup group, ItemContainerInfo container)
        {
            var result = new List<object>() {container};
            while (!(group is null || group is CollectionViewGroupRoot))
            {
                result.Insert(0, group);
                group = (group as CollectionViewGroupInternal)?.Parent;
            }

            return result;
        }


        private static void RemoveContainers(
            IItemsPresenter owner,
            IEnumerable<ItemContainerInfo> items)
        {
            foreach (var i in items)
            {
                if (i.ContainerControl != null)
                {
                    RemoveControl(owner, owner?.Panel, i.ContainerControl);
                }
            }
        }

        private static void RemoveControl(IItemsPresenter owner, IPanel panel, IControl control)
        {
            for (var i = panel.Children.Count - 1; i >= 0; i--)
            {
                var child = panel.Children[i];
                if (child == control)
                {
                    panel.Children.RemoveAt(i);
                    continue;
                }


                if (child is IGroupItem groupItem)
                {
                    RemoveControl(owner, groupItem.Panel, control);
                    CheckAndRemoveGroupItem(owner, panel, groupItem);
                }
            }
        }

        private static void CheckAndRemoveGroupItem(IItemsPresenter owner, IPanel panel, IGroupItem groupItem)
        {
            if (owner.Items is not ICollectionView collectionView)
                return;
            if (!groupItem.IsEmpty)
                return;
            if (groupItem.DataContext is not CollectionViewGroup viewGroup)
                return;
            if (collectionView.ContainsGroup(viewGroup))
                return;

            panel.Children.Remove(groupItem);
        }
        
        private static T GetParent<T>(IControl control) where T: class
        {
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
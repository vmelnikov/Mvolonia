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
                RemoveContainers(panel, generator.RemoveRange(e.OldStartingIndex, e.OldItems.Count));
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
                    RemoveContainers(panel, generator.Dematerialize(e.OldStartingIndex, e.OldItems.Count));
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
                    RemoveContainers(panel, generator.Clear());

                    if (items != null)
                    {
                        AddContainers(owner, 0, items);
                    }

                    break;
            }
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

                var group = collectionView.FindGroupContainingItem(item);
                if (!(group is null))
                {
                    var groupItem = panel.Children.OfType<GroupItem>()
                        .FirstOrDefault(c => Equals(c.ViewGroup, group));

                    if (groupItem is null)
                    {
                        groupItem = new GroupItem
                        {
                            ViewGroup = group
                        };
                        panel.Children.Add(groupItem);
                    }

                    groupItem.Panel.Children.Add(materialized.ContainerControl);
                }
                else
                {
                    panel.Children.Add(materialized.ContainerControl);
                }


                result.Add(materialized);
            }

            return result;
        }


        private static void RemoveContainers(
            IPanel panel,
            IEnumerable<ItemContainerInfo> items)
        {
            foreach (var i in items)
            {
                if (i.ContainerControl != null)
                {
                    RemoveControl(panel, i.ContainerControl);
                }
            }
        }

        private static void RemoveControl(IPanel panel, IControl control)
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
                    RemoveControl(groupItem.Panel, control);
                    if (groupItem.IsEmpty)
                        panel.Children.RemoveAt(i);
                }
            }
        }
    }
}
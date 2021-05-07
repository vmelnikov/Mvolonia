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
            var result = new List<ItemContainerInfo>();
            var panel = owner.Panel;

            foreach (var item in items)
            {
                var materialized = generator.Materialize(index++, item);

                if (owner.Items is ICollectionView collectionView)
                {
                    var group = collectionView.GroupingItems.Cast<CollectionViewGroup>()
                        .FirstOrDefault(g => g.Items.Contains(item));
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
                    panel.Children.Remove(i.ContainerControl);
                }
            }
        }
    }
}

using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.VisualTree;
using Mvolonia.Controls.Collections;
using Mvolonia.Controls.Presenters;
using Mvolonia.Controls.Tests.MockedObjects;
using Mvolonia.Controls.Tests.Models;
using Xunit;

namespace Mvolonia.Controls.Tests
{
    public class GroupingListBoxTests
    {
        [Fact]
        public void Should_Create_Three_GroupItems()
        {
            using (UnitTestApplication.Start(TestServices.MockPlatformRenderInterface))
            {
                var persons = new[]
                {
                    new Person("Ivan", "Ivanov", "Company1"),
                    new Person("Ivan", "Ivanov", "Company2"),
                    new Person("Ivan", "Ivanov", "Company3"),
                };
                var collectionView = new CollectionView(persons);
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Company"));


                var target = new GroupableListBox()
                {
                    Template = GroupingListBoxTemplate(),
                    Items = collectionView
                };

                Prepare(target);

                var groupItems = target.Presenter.Panel.Children.OfType<GroupItem>().ToList();
                Assert.Equal(3, groupItems.Count);
            }
        }

        [Fact]
        public void Should_Create_One_GroupItem()
        {
            using (UnitTestApplication.Start(TestServices.MockPlatformRenderInterface))
            {
                var persons = new[]
                {
                    new Person("Ivan", "Ivanov", "Company1"),
                    new Person("Ivan", "Ivanov", "Company1"),
                    new Person("Ivan", "Ivanov", "Company1"),
                };
                var collectionView = new CollectionView(persons);
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Company"));


                var target = new GroupableListBox()
                {
                    Template = GroupingListBoxTemplate(),
                    Items = collectionView
                };

                Prepare(target);

                var groupItems = target.Presenter.Panel.Children.OfType<GroupItem>().ToList();
                Assert.Single(groupItems);
            }
        }
        
        [Fact]
        public void Should_Not_Create_Any_GroupItems()
        {
            using (UnitTestApplication.Start(TestServices.MockPlatformRenderInterface))
            {
                var persons = new[]
                {
                    new Person("Ivan", "Ivanov", "Company1"),
                    new Person("Ivan", "Ivanov", "Company2"),
                    new Person("Ivan", "Ivanov", "Company3"),
                };
                var collectionView = new CollectionView(persons);


                var target = new GroupableListBox()
                {
                    Template = GroupingListBoxTemplate(),
                    Items = collectionView
                };

                Prepare(target);

                var groupItems = target.Presenter.Panel.Children.OfType<GroupItem>().ToList();
                Assert.Empty(groupItems);
            }
        }

        private static void Prepare(ItemsControl target)
        {
            // The ListBox needs to be part of a rooted visual tree.
            var testRoot = new TestRoot {Child = target};

            // Apply the template to the ListBox itself.
            target.ApplyTemplate();

            // Then to its inner ScrollViewer.
            var scrollViewer = (ScrollViewer) target.GetVisualChildren().Single();
            scrollViewer.ApplyTemplate();

            // Then make the ScrollViewer create its child.
            ((ContentPresenter) scrollViewer.Presenter).UpdateChild();

            // Now the ItemsPresenter should be registered, so apply its template.
            target.Presenter.ApplyTemplate();

            // Because ListBox items are virtualized we need to do a layout to make them appear.
            target.Measure(new Size(100, 100));
            target.Arrange(new Rect(0, 0, 100, 100));

            // Now set and apply the item templates.
            foreach (var item in target.Presenter.Panel.Children)
            {
                if (item is ListBoxItem listBoxItem)
                {
                    listBoxItem.Template = ListBoxItemTemplate();
                    listBoxItem.ApplyTemplate();
                    listBoxItem.Presenter.ApplyTemplate();
                    ((ContentPresenter) listBoxItem.Presenter).UpdateChild();
                }
            }

            // The items were created before the template was applied, so now we need to go back
            // and re-arrange everything.
            // foreach (IControl i in target.GetSelfAndVisualDescendants())
            // {
            //     i.InvalidateMeasure();
            // }

            target.Measure(new Size(100, 100));
            target.Arrange(new Rect(0, 0, 100, 100));
        }

        private static FuncControlTemplate ListBoxItemTemplate()
        {
            return new FuncControlTemplate<ListBoxItem>((parent, scope) =>
                new ContentPresenter
                {
                    Name = "PART_ContentPresenter",
                    [!ContentPresenter.ContentProperty] = parent[!ContentControl.ContentProperty],
                    [!ContentPresenter.ContentTemplateProperty] = parent[!ContentControl.ContentTemplateProperty],
                }.RegisterInNameScope(scope));
        }

        private FuncControlTemplate GroupingListBoxTemplate()
        {
            return new FuncControlTemplate<GroupableListBox>((parent, scope) =>
                new ScrollViewer
                {
                    Name = "PART_ScrollViewer",
                    Template = ScrollViewerTemplate(),
                    Content = new GroupableItemsPresenter
                    {
                        Name = "PART_ItemsPresenter",
                        [~ItemsPresenterBase.ItemsProperty] =
                            parent.GetObservable(ItemsControl.ItemsProperty).ToBinding(),
                        [~ItemsPresenterBase.ItemsPanelProperty] =
                            parent.GetObservable(ItemsControl.ItemsPanelProperty).ToBinding(),
                        VirtualizationMode = ItemVirtualizationMode.None
                        //[~GroupableItemsPresenter.VirtualizationModeProperty] = parent.GetObservable(GroupableListBox.VirtualizationModeProperty).ToBinding(),
                    }.RegisterInNameScope(scope)
                }.RegisterInNameScope(scope));
        }

        private static FuncControlTemplate ScrollViewerTemplate()
        {
            return new FuncControlTemplate<ScrollViewer>((parent, scope) =>
                new Panel
                {
                    Children =
                    {
                        new ScrollContentPresenter
                        {
                            Name = "PART_ContentPresenter",
                            [~ContentPresenter.ContentProperty] =
                                parent.GetObservable(ContentControl.ContentProperty).ToBinding(),
                            [~~ScrollContentPresenter.ExtentProperty] = parent[~~ScrollViewer.ExtentProperty],
                            [~~ScrollContentPresenter.OffsetProperty] = parent[~~ScrollViewer.OffsetProperty],
                            [~~ScrollContentPresenter.ViewportProperty] = parent[~~ScrollViewer.ViewportProperty],
                        }.RegisterInNameScope(scope),
                        new ScrollBar
                        {
                            Name = "verticalScrollBar",
                            [~RangeBase.MaximumProperty] = parent[~ScrollViewer.VerticalScrollBarMaximumProperty],
                            [~~RangeBase.ValueProperty] = parent[~~ScrollViewer.VerticalScrollBarValueProperty],
                        }
                    }
                });
        }
    }
}
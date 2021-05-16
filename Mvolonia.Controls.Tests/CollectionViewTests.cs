using Mvolonia.Controls.Collections;
using Mvolonia.Controls.Tests.Models;
using Xunit;

namespace Mvolonia.Controls.Tests
{
    public class CollectionViewTests
    {
        [Fact]
        public void IsGrouping_Should_Be_True_When_GroupDescriptions_Is_Not_Empty()
        {
            var persons = new[]
            {
                new Person("Ivan", "Ivanov", "Company1"),
            };
            var collectionView = new CollectionView(persons);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Company"));
            Assert.True( collectionView.IsGrouping);
        }
        
        [Fact]
        public void IsGrouping_Should_Be_False_When_GroupDescriptions_Is_Empty()
        {
            var persons = new[]
            {
                new Person("Ivan", "Ivanov", "Company1"),
            };
            var collectionView = new CollectionView(persons);
            Assert.False( collectionView.IsGrouping);
        }      
      
    }
}
using System.Collections;
using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ControlCatalog.Models;
using Mvolonia.Controls;
using Mvolonia.Controls.Collections;

namespace ControlCatalog.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            var listBox = this.FindControl<GroupingListBox>("ListBox");

            
            var collectionView = new CollectionView(CreateEmployees());
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Organisation"));

            listBox.Items = collectionView;
        }

        private static IEnumerable CreateEmployees()
        {
            var builder = ImmutableList.CreateBuilder<Employee>();
            builder.Add(new Employee("Ivan", "Ivanov", "Roga & Kopyta"));
            builder.Add(new Employee("Petr", "Petrov", "Roga & Kopyta"));
            builder.Add(new Employee("Sidor", "Sidorov", "Roga & Kopyta"));
            builder.Add(new Employee("Petr", "Ivanov", "Only Roga"));
            builder.Add(new Employee("Ivan", "Petrov", "Only Kopyta"));
            return builder.ToImmutable();
        }
    }
}
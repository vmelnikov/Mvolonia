using System.Collections;
using System.Linq;
using Avalonia.Collections;
using Bogus;
using Bogus.DataSets;
using ControlCatalog.Models;
using Mvolonia.Controls.Collections;
using ReactiveUI;

namespace ControlCatalog.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly AvaloniaList<Employee> _employees = new AvaloniaList<Employee>();
        private CollectionView _groupedEmployees = null!;

        public MainWindowViewModel()
        {
            FillDefaultEmployees();
            GroupedEmployees = CreateGroupedEmployees(_employees);
        }

        public AvaloniaList<Employee> SelectedItems { get; } = new AvaloniaList<Employee>();

        public CollectionView GroupedEmployees
        {
            get => _groupedEmployees;
            set => this.RaiseAndSetIfChanged(ref _groupedEmployees, value);
        }

        private static CollectionView CreateGroupedEmployees(IEnumerable employees)
        {
            var collectionView = new CollectionView(employees);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Company"));
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Gender"));
            return collectionView;
        }

        private void DeleteSelected()
        {
            _employees.RemoveAll(SelectedItems.ToList());
        }

        public void OnDeleteSelectedCommand() =>
            DeleteSelected();

        public void OnAddToRandomGroupCommand() =>
            AddToRandomGroup();

        private void AddToRandomGroup()
        {
            var faker = new Faker<Employee>()
                .CustomInstantiator(f => new Employee(f.Name.FirstName(), f.Name.LastName(), f.Company.CompanyName(),
                    f.Person.Gender == Name.Gender.Male ? "Male" : "Female"));
            _employees.Add(faker.Generate());
        }

        public void AddNewPersonCommand(string company)
        {
            var faker = new Faker<Employee>()
                .CustomInstantiator(f => new Employee(f.Name.FirstName(), f.Name.LastName(), company,
                    f.Person.Gender == Name.Gender.Male ? "Male" : "Female"));
            _employees.Add(faker.Generate());
        }

        private void FillDefaultEmployees()
        {
            _employees.Add(new Employee("Ivan", "Ivanov", "Roga & Kopyta", "Male"));
            _employees.Add(new Employee("Petr", "Petrov", "Roga & Kopyta", "Male"));
            _employees.Add(new Employee("Raisa", "Ivanovna", "Roga & Kopyta", "Female"));
            _employees.Add(new Employee("Petr", "Ivanov", "Only Roga", "Male"));
            _employees.Add(new Employee("Elena", "Petrovna", "Only Kopyta", "Female"));
        }
    }
}
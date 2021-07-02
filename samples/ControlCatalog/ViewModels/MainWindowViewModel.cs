using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly AvaloniaList<Employee> _employees = new();
        private CollectionView _groupedEmployees = null!;

        public MainWindowViewModel()
        {
            GroupedEmployees = CreateGroupedEmployees(_employees);
            FillDefaultEmployees();
            SelectedItems.CollectionChanged += (sender, args) => { };
        }

        public AvaloniaList<Employee> SelectedItems { get; } = new();

        public CollectionView GroupedEmployees
        {
            get => _groupedEmployees;
            set => this.RaiseAndSetIfChanged(ref _groupedEmployees, value);
        }

        private static CollectionView CreateGroupedEmployees(IEnumerable employees)
        {
            var companyGroupDescription = new PropertyGroupDescription(nameof(Employee.Company));
            companyGroupDescription.GroupNames.Add("Empty Company");

            var genderGroupDescription = new PropertyGroupDescription(nameof(Employee.Gender));
            genderGroupDescription.GroupNames.Add("Male");
            var collectionView = new CollectionView(employees);
            collectionView.GroupDescriptions.Add(companyGroupDescription);
            collectionView.GroupDescriptions.Add(genderGroupDescription);
            collectionView.SortDescriptions.Add(SortDescription.FromPropertyName(nameof(Employee.SecondName),
                ListSortDirection.Descending));
            return collectionView;
        }

        private void DeleteSelected()
        {
            _employees.RemoveAll(SelectedItems.ToList());
        }

        public void OnDeleteSelectedCommand() =>
            DeleteSelected();

        public void OnChangeSelectedSecondNameCommand() =>
            ChangeSelectedSecondName();

        public void OnAddToRandomGroupCommand() =>
            AddToRandomGroup();

        private void AddToRandomGroup()
        {
            var faker = new Faker<Employee>()
                .CustomInstantiator(f => new Employee(f.Name.FirstName(), f.Name.LastName(), f.Company.CompanyName(),
                    f.Person.Gender == Name.Gender.Male ? "Male" : "Female"));
            _employees.Add(faker.Generate());
        }

        private void ChangeSelectedSecondName()
        {
            var faker = new Faker<string>().CustomInstantiator(f => f.Name.LastName());
            var selectedItems = SelectedItems.ToList();
            foreach (var selectedItem in selectedItems)
            {
                var index = _employees.IndexOf(selectedItem);
                var newItem = new Employee(selectedItem.FirstName, faker.Generate(), selectedItem.Company,
                    selectedItem.Gender);
                _employees[index] = newItem;
            }
        }

        public void AddNewMaleCommand(string company)
        {
            var faker = new Faker<Employee>()
                .CustomInstantiator(f => new Employee(f.Name.FirstName(), f.Name.LastName(), company, "Male"));
            _employees.Add(faker.Generate());
        }

        public void AddNewFemaleCommand(string company)
        {
            var faker = new Faker<Employee>()
                .CustomInstantiator(f => new Employee(f.Name.FirstName(), f.Name.LastName(), company, "Female"));
            _employees.Add(faker.Generate());
        }

        private void FillDefaultEmployees()
        {
            var toAdd = new List<Employee>
            {
                new Employee("Ivan", "Ivanov", "Roga & Kopyta", "Male"),
                new Employee("Petr", "Petrov", "Roga & Kopyta", "Male"),
                new Employee("Raisa", "Ivanovna", "Roga & Kopyta", "Female"),
                new Employee("Petr", "Ivanov", "Only Male Company", "Male"),
                new Employee("Elena", "Petrovna", "Only Female Company", "Female")
            };
            _employees.AddRange(toAdd);
        }
    }
}
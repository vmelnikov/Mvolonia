using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;
using ControlCatalog.Models;
using Microsoft.CodeAnalysis.Operations;
using Mvolonia.Controls.Collections;
using ReactiveUI;

namespace ControlCatalog.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        
        private readonly ObservableCollection<Employee> _employees = new ObservableCollection<Employee>();
        private CollectionView _groupedEmployees;

        public MainWindowViewModel()
        {
            FillDefaultEmployees();
            GroupedEmployees = CreateGroupedEmployees(_employees);

        }


        public CollectionView GroupedEmployees
        {
            
            get => _groupedEmployees;
            set => this.RaiseAndSetIfChanged(ref _groupedEmployees, value);
        }

        private static CollectionView CreateGroupedEmployees(IEnumerable employees)
        {
            var collectionView = new CollectionView(employees);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Organisation"));
            return collectionView;
        }

        public void AddNewPersonCommand(string organisation)
        {
            _employees.Add(new Employee("Name", "Surname", organisation));
        }
        
        private void FillDefaultEmployees()
        {
            _employees.Add(new Employee("Ivan", "Ivanov", "Roga & Kopyta"));
            _employees.Add(new Employee("Petr", "Petrov", "Roga & Kopyta"));
            _employees.Add(new Employee("Sidor", "Sidorov", "Roga & Kopyta"));
            _employees.Add(new Employee("Petr", "Ivanov", "Only Roga"));
            _employees.Add(new Employee("Ivan", "Petrov", "Only Kopyta"));
        }
    }
}
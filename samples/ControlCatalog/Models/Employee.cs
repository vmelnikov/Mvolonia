namespace ControlCatalog.Models
{
    public class Employee
    {
        public Employee(string name, string surname, string organisation)
        {
            Name = name;
            Surname = surname;
            Organisation = organisation;
        }

        public string Organisation { get; }

        public string Surname { get; }

        public string Name { get; set; }
    }       
}
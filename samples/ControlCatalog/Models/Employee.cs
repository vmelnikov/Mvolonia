namespace ControlCatalog.Models
{
    public class Employee
    {
        public Employee(string firstName, string secondName, string company, string gender)
        {
            FirstName = firstName;
            SecondName = secondName;
            Company = company;
            Gender = gender;
        }

        public string Company { get; }

        public string SecondName { get; set; }

        public string FirstName { get; }

        public string Gender { get; }
    }
}
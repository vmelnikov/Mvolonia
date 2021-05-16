namespace Mvolonia.Controls.Tests.Models
{
    public class Person
    {
        public Person(string firstName, string secondName, string company)
        {
            FirstName = firstName;
            SecondName = secondName;
            Company = company;
        }
        
        public string FirstName { get; }
        
        public string SecondName { get; }
        
        public string Company { get; }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace TacoFlix.Model
{
    public class Person
    {
        public Guid Identifier { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string EmailAddress { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TacoFlix.Model;
using TacoFlix.Xconnect.Model;

namespace TacoFlix.Client.Generators.Contacts
{
    public class ContactGenerator : IContactGenerator
    {
        private readonly IRandomGenerator _randomGenerator;

        private readonly List<string> _maleFirstNames = new List<string>();
        private readonly List<string> _femaleFirstNames = new List<string>();
        private readonly List<string> _lastNames = new List<string>();
        private readonly List<string> _domains = new List<string>();

        public ContactGenerator(IRandomGenerator randomGenerator)
        {
            _randomGenerator = randomGenerator;

            _maleFirstNames.AddRange(ReadFile("App_Data\\male.txt"));
            _femaleFirstNames.AddRange(ReadFile("App_Data\\female.txt"));
            _lastNames.AddRange(ReadFile("App_Data\\surnames.txt"));
            _domains.AddRange(ReadFile("App_Data\\domains.txt"));
        }

        public Person CreateContact()
        {
            var fakeContact = new Person
            {
                Identifier = Guid.NewGuid()
            };

            if (_randomGenerator.GetInteger(1, 2) % 2 == 0)
            {
                fakeContact.FirstName = GetMaleFirstName();
                fakeContact.Gender = "Male";
            }
            else
            {
                fakeContact.FirstName = GetFemaleFirstName();
                fakeContact.Gender = "Female";
            }

            fakeContact.LastName = GetLastName();
            fakeContact.EmailAddress = $"{fakeContact.FirstName.ToLower()}.{fakeContact.LastName.ToLower()}@{GetDomainName()}";
            return fakeContact;
        }

        private string GetMaleFirstName()
        {
            return _maleFirstNames[_randomGenerator.GetInteger(0, _maleFirstNames.Count)];
        }

        private string GetFemaleFirstName()
        {
            return _femaleFirstNames[_randomGenerator.GetInteger(0, _femaleFirstNames.Count)];
        }

        private string GetLastName()
        {
            return _lastNames[_randomGenerator.GetInteger(0, _lastNames.Count)];
        }

        private string GetDomainName()
        {
            return _domains[_randomGenerator.GetInteger(0, _domains.Count)];
        }

        private List<string> ReadFile(string fileName)
        {
            var data = new List<string>();

            var lines = File.ReadAllLines(fileName);
            foreach (var line in lines)
            {
                data.Add(line);
            }

            return data;
        }
    }
}

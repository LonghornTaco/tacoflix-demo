using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TacoFlix.Model;
using TacoFlix.Xconnect.Model;

namespace TacoFlix.Client.Generators.Contacts
{
    public interface IContactGenerator
    {
        Person CreateContact();
    }
}

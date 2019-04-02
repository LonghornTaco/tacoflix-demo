using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TacoFlix.Client.Generators
{
    public interface IRandomGenerator
    {
        int GetInteger(int min, int max);
        decimal GetDecimal(int min, int max);
    }
}

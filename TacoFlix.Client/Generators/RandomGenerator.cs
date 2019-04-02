using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TacoFlix.Client.Generators
{
    public class RandomGenerator : IRandomGenerator
    {
        private readonly Random _random = new Random();

        public int GetInteger(int min, int max)
        {
            return _random.Next(min, max);
        }

        public decimal GetDecimal(int min, int max)
        {
            return decimal.Parse($"{_random.Next(min, max)}.{_random.Next(10, 99)}");
        }
    }
}

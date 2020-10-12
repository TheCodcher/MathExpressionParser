using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeParseSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new RealNumbersExpressionParser();
            var s = "-(cos(a+b))^k-(sin(c*d))^n"; //ответ -1
            var dict = new Dictionary<string, double>()
            {
                {"a",3 },
                {"b",3 },
                {"c",2 },
                {"d",3 },
                {"k",2 },
                {"n",2 },
            };
            var result = parser.Parse(s, dict);
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}

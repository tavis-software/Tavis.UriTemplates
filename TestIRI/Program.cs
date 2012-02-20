using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestIRI
{
    class Program
    {
        static void Main(string[] args)
        {
            var reserved3986chars = "-._~";
            var reserved2396chars = "-_.!~*'()";

            var result = Uri.EscapeUriString(reserved3986chars);
            var result2 = Uri.EscapeUriString(reserved2396chars);

            Console.WriteLine(result);
            Console.WriteLine(result2);
            Console.Read();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Slowsharp
{
    internal class C
    {
        [Conditional("SS_TRACE")]
        public static void Write(string message)
        {
            Console.Write(message);
        }
        [Conditional("SS_TRACE")]
        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}

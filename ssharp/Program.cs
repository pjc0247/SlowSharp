using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Slowsharp;

namespace ssharp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Please provide input file.");
                    Console.WriteLine("   ssharp a.cs b.cs c.cs");
                    return;
                }

                var srcs = new List<string>();
                foreach (var arg in args)
                    srcs.Add(File.ReadAllText(arg));

                var cs = CScript.CreateRunner(srcs.ToArray());
                cs.RunMain();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

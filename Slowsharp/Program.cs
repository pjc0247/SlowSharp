using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var src = @"
using System;
using System.Collections;
using System.Linq;
using System.Text;
 
namespace HelloWorld
{
    class Bar {
        public int a;
        public Bar() {
            a = 1234;
            Console.WriteLine(1234);
        }
    }
    class Program
    {
static int Foo() { return 5; }

        static void Main(string[] args)
        {
            var a = new Bar();
        }
    }
}
";
            var tree = CSharpSyntaxTree.ParseText(src);
            var root = tree.GetCompilationUnitRoot();

            Dump(root);

            var r = new Runner(Assembly.GetEntryAssembly(), new RunConfig());
            r.Run(root);
            r.RunMain();
        }

        private static void Dump(SyntaxNode syntax, int depth = 0)
        {
            for (int i = 0; i < depth; i++) Console.Write("  ");
            Console.WriteLine(syntax.GetType());

            foreach (var child in syntax.ChildNodes())
                Dump(child, depth + 1);
        }
    }
}

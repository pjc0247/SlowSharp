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
    class Bar
    {
        public void Boo()
        {
            Console.WriteLine(1234);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var src = @"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
namespace HelloWorld
{
    class Foo {
        private int aa = 55;

private static int Boo() => 5;

        public Foo() {
            Console.WriteLine(1234);
        }
        public Foo(int b) {
            Console.WriteLine(11);
        }
    }
    class Program
    {
static int bb = 1;
static int Foo() { return 5; }
static int Foo(int n) { return 15; }

private static int Booo() => 5;

static void Bbb(params object[] obj) {
foreach (var b in obj)
Console.WriteLine(b);
}

        static int Main(int n)
        {

var a = 4;
object b = (vve)a;

Console.WriteLine(b.GetType());

return r;
        }

static void Bo() {
var aa = 1234;
Console.WriteLin(aa);
}
    }
}
";

            Console.WriteLine();
            var tree = CSharpSyntaxTree.ParseText(src);
            var root = tree.GetCompilationUnitRoot();

            Dump(root);

            var r = new Runner(Assembly.GetEntryAssembly(), new RunConfig() {
            });
            r.Run(root);
            var ret = r.RunMain(5).innerObject;

            Console.WriteLine(ret);
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

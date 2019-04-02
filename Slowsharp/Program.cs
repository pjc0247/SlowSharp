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

    struct St
    {
        public int a;
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
    class Fooo {
        public int aa = 55;

public int Boo { get; set; }

        public void Foo() {
            Console.WriteLine(this);
        }
        public void Foo(int b) {
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
Fooo.aa = 5455;
Console.WriteLine(Fooo.aa);

return r;
        }

static void Bo() {
var aa = 1234;
Console.WriteLin(aa);
}
    }
}
";
            var tree = CSharpSyntaxTree.ParseText(src);
            var root = tree.GetCompilationUnitRoot();

            Dump(root);

            var r = new Runner(new RunConfig() {
            });
            r.Run(root);
            var ret = r.RunMain(5).innerObject;

            dynamic d = new DynamicHybInstance(r.Instantiate("Fooo"));
            d.Foo();
            //Console.WriteLine(r.Instantiate("Fooo").Invoke("Foo", 1));
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

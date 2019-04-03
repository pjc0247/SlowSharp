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
        public int ff = 122;

        public Bar()
        {
            Console.WriteLine("I AM INIT");
        }

        public void SayHello()
        {
            Console.WriteLine("hello");
        }
        public static void Boo(out object b)
        {
            b = 1122;
            Console.WriteLine(1234);
        }
    }

    struct St
    {
        public static object a;
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
    class Fooo : Bar {
        public static int aa = 1;

public static int Boo { get; set; } = 11;

        public void Foo() {
            Console.WriteLine(Boo);
        }
        public void Foo(int b) {
            Console.WriteLine(11);
        }
    }
    class Program : Bar
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

int a = 11;
Bar.Boo(out a);

Console.WriteLine(a);

return CScript.RunSimple(""55"", null);

//return ""asdf"";
        }

static void Bo() {
var aa = 1234;
Console.WriteLin(aa);
}
    }
}
";
            Bar.Boo(out St.a);

            Console.WriteLine(CScript.RunSimple("\"hello from inception\""));

            Console.WriteLine(src);

            var tree = CSharpSyntaxTree.ParseText(src);
            var root = tree.GetCompilationUnitRoot();

            Dump(root);

            var r = new Runner(new RunConfig() {
            });
            r.Run(root);
            //var ret = r.RunMain(5).innerObject;

            //Console.WriteLine(ret);

            var bar = new Bar();
            dynamic d = new DynamicHybInstance(r.Override("Fooo", bar));
            //d.Foo();

            d.SayHello();
            Console.WriteLine(d.ff);
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

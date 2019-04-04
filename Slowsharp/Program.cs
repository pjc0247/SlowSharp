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
    public struct Vector3
    {
        public int x, y, z;
        public Vector3(int x, int y, int z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
    }
    public class Transform
    {
        public Vector3 position { get; set; }
    }

    public class Bar
    {
        public Transform transform = new Transform();

        protected int ff = 122;

        public Bar()
        {
            Console.WriteLine("I AM INIT");
        }

        public void SayHello()
        {
            Console.WriteLine("hello from BAR");
        }
        public static void SayHelloStatic()
        {
            Console.WriteLine("hello");
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

        public int FF { get { return ff; } }        
        
        public void Foo() {
            Console.WriteLine(Boo);
        }
        public void Foo(int b) {
            Console.WriteLine(11);
        }
    }

class Boor : Bar {
public void MoveForward() {  
    transform.position += new Vector3(11,1,1);
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



        static int Main(int n) {

    var p = new Boor();
p.Add(1, 1);
Console.WriteLine(p[1]);

var a = new Dictionary<int, int>() { {1, 1}, {2, 2}, {3, 3} };
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
            //dynamic d = new DynamicHybInstance(r.Override("Boor", bar));
            //d.Boo();
            //d.SayHello();
            var boor = r.Override("Boor", bar);
            boor.Invoke("MoveForward");

            Console.WriteLine(bar.transform.position.x);

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

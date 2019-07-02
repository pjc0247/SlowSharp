using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Slowsharp
{
    enum KeyCode
    {
        Space
    }
    interface IFoo { }
    class Foo
    {
        public int value;
        public static Foo operator +(Foo a, Foo b)
        {
            return new Foo() { value = a.value + b.value };
        }
    }
    class Input
    {
        public static bool GetKeyDown(KeyCode k)
        {
            return true;
        }
    }
    struct Vector3
    {
        public int x, y, z;
        public Vector3(int x, int y, int z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
    }
    class Transform
    {
        public Vector3 position { get; set; }
    }

    class Resources
    {
        public static void Load<T>(string path)
        {
            Console.WriteLine(typeof(T) + " / " + path);
        }
    }
    class GameObject
    {
    }

    class Bar
    {
        public Transform transform = new Transform();

        protected int ff = 122;

        public Bar()
        {
            Console.WriteLine("I AM INIT");
        }

        public static void Opt(int n = 1)
        {
            Console.WriteLine("Opt" + n);
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
    public class RefOrOutTest
    {
        public static void MakeDoubleOut(int input, out int value)
        {
            value = input * 2;
        }
        public static void MakeDoubleRef(ref int value)
        {
            value *= 2;
        }
    }
    struct St
    {
        public static object a;
    }

    public class SSDebugger
    {
        public static CScript runner;

        public static void Stop()
        {
            var dump = runner.GetDebuggerDump();

            while (true)
            {
                Console.WriteLine("<<");
                var src = Console.ReadLine();
                Console.WriteLine(">>");
                Console.WriteLine(runner.Eval(src));
            }

            ;
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
using Slowsharp;
 
namespace HelloWorld
{

    class Fooo : Bar {
        public static int aa = 1;

        public int FF { get { return ff; } }        
        
        public void Foo() {
            Console.WriteLine(1);
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
class MyList : List<int> { }
    class Program : Bar, IFoo
    {
static int bb = 1;
static int Foo() { return 5; }
static int Foo(int n) { return 15; }

public static int Booo(int n = 5) => 5;

static IEnumerator Bbb() {
yield return 1;
yield return 2;
yield return 3;
}

private static int acac = 123;

        static int Main(int n) {

var c = new Action<int>((int a) => { Console.WriteLine(a); });
c.Invoke(1234879);

return 1;

var a = new int[] {1,2,3,4}
return a.Any((int x) => { return x > 2; });

return v;

return 0;
//return ""asdf"";
        }

static void Bo() {
var aa = 1234;
Console.WriteLin(aa);
}
    }
}
";

            var hotReloadSrc = @"
class Fooo : Bar {
    public void Foo() {
        Console.WriteLine(12341234);
    }
}
";
            //src = System.IO.File.ReadAllText("a.cs");

            Console.WriteLine(nameof(Console));
            Console.WriteLine(CScript.RunSimple("\"hello from inception\""));

            Console.WriteLine(src);

            var config = ScriptConfig.Default;

            config.PrewarmTypes = new Type[] { typeof(Console) };

            var run = CScript.CreateRunner(src, config);
            SSDebugger.runner = run;

            run.Trap(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) }),
                typeof(Program).GetMethod("NewWriteLine"));

            var myList = run.RunMain();
            Console.WriteLine(myList.Is<List<int>>());

            return;
        }

        public static void NewWriteLine(int n)
        {
            Console.WriteLine(n * 2);
        }

        private static void Any<T1, T2>(IEnumerable<T1> f, Func<T2, bool> ff)
        {

        }

        

        private static int Goo()
        {
            var count = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    count += 10;
                    //if (j == 2) break;
                }
                count++;
                //if (i == 2) break;
            }
            return count;
        }
    }
}

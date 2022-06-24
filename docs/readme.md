SlowSharp
====

Hello World
----
```cs
Console.WriteLine( CScript.RunSimple("\"hello from SlowSharp!\"") );
```

Basic Usage
----
__Read from disk and execute it__
```cs
// test.cs
class Program {
    public static void Main() 
        => Console.WriteLine("Hello World");
}
```
```cs
var src = File.ReadAllText("test.cs");
var runner = CScript.CreateRunner(src);

runner.RunMain();
```


__Instantiate by class name__
```cs
class Foo {
    public void Hello() 
        => Console.WriteLine("Hello World");
}
```
```cs
var foo = runner.Instantiate("Foo");
foo.Invoke("Hello");
```

__Reflection style__
```cs
class Foo {
    public static void Hello() 
        => Console.WriteLine("Hello World");
}
```
```cs
var fooType = runner.GetTypes()
      .Where(x => x.Name == "Foo")
      .First();

fooType.GetStaticMethods("Hello")
      .First()
      .Invoke();
```

Script Debugger
----
Demonstrate how to use script debugging API and create a simple REPL.
```cs
// script_host.cs
public class SSDebugger
{
    public static CScript runner;

    public static void Stop()
    {
        var dump = runner.GetDebuggerDump();

        // Read Eval Print Loop
        while (true)
        {
            Console.WriteLine("<<");
            var src = Console.ReadLine();
            Console.WriteLine(">>");
            Console.WriteLine(runner.Eval(src));
        }
    }
}
```
```cs
// test.csx

var a = 1234;
var b = "hello";

SSDebugger.Stop();
```

__RESULT__
```
<< a
>> 1234

<< b
>> hello
```

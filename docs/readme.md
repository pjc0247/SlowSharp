SlowSharp
====

Hello World
----
```cs
Console.WriteLine( CScript.RunSimple("\"hello from SlowSharp!\"") );
```

Run with file
----
__test.cs__
```cs
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


__test.cs__
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

__test.cs__
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
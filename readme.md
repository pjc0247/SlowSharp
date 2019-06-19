SlowSharp [![Build Status](https://travis-ci.com/pjc0247/SlowSharp.svg?branch=master)](https://travis-ci.com/pjc0247/SlowSharp)
====

Slow version of C# interpreter because modern JITs are too fast.

Pros
----
* __More Compatability__: Brings C# scripting into __WSA__, __iOS__ and __WebAssembly__.
* __And nothing__


Zero-binding
----
* No manual assembly references
* No type or method binding

Even this can be possible:
```cs
CScript.Run(@"
   CScript.Run("" Console.WriteLine(""hello from inception""); "");
");
```

Hot Reloading
----
Can replace methods after parsing. This also affects already instantiated objects. 
```cs
var r = CScript.CreateRunner(@"
class Foo { public int GiveMeNumber() => 10; }
");

var foo = r.Instantiate("Foo");
// should be 10
foo.Invoke("GiveMeNumber");
```
```cs
ss.UpdateMethodsOnly(@"
class Foo { public int GiveMeNumber() => 20; }
");
// should be 20
foo.Invoke("GiveMeNumber");
```

Overriding (Virtual inheritance)
----
Supports virtual inheritance, this is useful with __Unity__.
```cs
class MyBehaviour : MonoBehaviour { /* ... */ }
```
```cs
// Error, MonoBehaviour cannot be instantiated by Unity's law
ss.Instantiate("MyBehaviour");

// Use this when you're not able to create an object yourself.
ss.Override("MyBehaviour", gameObject);
```

Finally, there will be two instances, but act as derivered one object.

Sandboxing
----
### Access control
Prevents malicious function call such as `Threading` and `File`.
```cs
var ac = new AccessControl();
ac.AddBlockedType("System.Threading.Thread");
ac.AddBlockedNamespace("System.IO");

new RunConfig() {
  accessControl = ac
};
```

### Timeout
Prevents long-running tasks with a timeout option.
```cs
new RunConfig() {
  timeout = 1000 /* ms */
};
```


Limitations
----
Since __SlowSharp__ is an interpreter, there're some differences and limitations.

__Lazy semantic validation__<br>
__SlowSharp__ does not validate whole syntaxs during the parse time. It only checks the overall form is valid or not.<br>
As a result, some mistakes won't be detected until it's fully reached and executed.
```cs
Console.WriteLine("Hello");

// There's not method `WriteLn` in `Conosle` class.
// However, this will not throw an exception util SlowSharp actually 
// evaluates the below line.
Console.WriteLn("World");
```
```cs
var a = 1234;
Console.WriteLine(a);
var a = 4556; // redefination
```

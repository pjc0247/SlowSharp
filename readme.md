SlowSharp
====

Slow version of C# interpreter because modern JIT is too fast.

Pros
----
* __More Compatability__: Brings C# scripting into __WSA__, __iOS__ and __WebAssembly__.
* __And nothing__


Limitation
----
Since __SlowSharp__ is an interpreter, there're some differences and limitations.

```cs
Console.WriteLine("Hello");
Console.WriteLn("World");
```

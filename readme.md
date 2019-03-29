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

Sandboxing
----
### Access control
```cs
var ac = new AccessControl();
ac.AddBlockedType("System.Threading.Thread");
ac.AddBlockedNamespace("System.IO");

new RunConfig() {
  accessControl = ac
};
```

### Timeout
```cs
new RunConfig() {
  timeout = 1000 /* ms */
};
```

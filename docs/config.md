Configs
====

ScriptConfig
----
__ScriptConfig__ contains properties which related to compile-time options.

__string[] Defaul Usings__<br>

```cs
config.DefaultUsings = new string[] { "System", "System.Collections.Generic" };
```

__Type[] PrewarmTypes__<br>
SlowSharp resolves type information in runtime, this is done by `Reflection` and may causes performance spikes.<br>
You can specify types to preload some types and prevent it.
```cs
config.PrewarmTypes = Type[] { typeof(System.Console), typeof(System.Math) };
```


RunConfig
----
__RunConfig__ contains properties which related to run-time options.

__int Timeout__<br>
```cs
config.Timeout = 3000; // ms
```

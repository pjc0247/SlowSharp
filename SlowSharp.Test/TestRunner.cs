﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp.Test
{
    public class TestRunner
    {
        public static RunConfig config = new RunConfig();

        public static object RunRaw(string code)
        {
            return CScript.Run(@"
using System;
" + code,
        config);
        }
        public static object Run(string code)
        {
            return CScript.Run(@"
using System;

public class Foo {

public static object Main() {
"
+ code +
@"
}
}",
config);
        }

        public static object Run(string classBody, string body)
        {
            return CScript.Run(@"
using System;

public class Foo {"
+ classBody +
@"
public static object Main() {
"
+ body +
@"
}
}",
config);
        }
    }
}

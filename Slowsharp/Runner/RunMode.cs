using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal enum RunMode
    {
        Preparse,
        Parse,
        Execution,
        HotLoadMethodsOnly
    }
}

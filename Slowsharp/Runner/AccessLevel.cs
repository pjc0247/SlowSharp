using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    /// <summary>
    /// Represents target member is being accessed by.
    /// </summary>
    internal enum AccessLevel
    {
        This, 
        Derivered,
        Outside
    }
}

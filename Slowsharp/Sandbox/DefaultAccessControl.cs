using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class DefaultAccessControl : IAccessFilter
    {
        public bool IsAllowedNamespace(string ns) => true;
        public bool IsSafeType(HybType type) => true;
    }
}

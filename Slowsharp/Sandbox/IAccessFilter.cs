using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public interface IAccessFilter
    {
        bool IsSafeType(HybType type);
        bool IsAllowedNamespace(string ns);
    }
}

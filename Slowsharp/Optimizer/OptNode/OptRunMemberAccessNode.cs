using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal struct OptRunMemberAccessNode : OptNodeBase
    {
        public bool isStaticMemberAccess;
        public HybType leftType;
    }
}

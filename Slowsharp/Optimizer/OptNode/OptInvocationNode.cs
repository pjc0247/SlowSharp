using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal struct OptInvocationNode : OptNodeBase
    {
        public HybType LeftType;
    }
    internal enum InvocationType
    {
        VarInvoke,
        LocalMethod,
        RemoteStaticMethod,
        RemoteMethod,
        PredefinedTypeStatic,
        ExpressionMethod
    }
}

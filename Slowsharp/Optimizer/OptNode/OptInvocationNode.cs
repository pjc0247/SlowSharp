using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal struct OptInvocationNode : OptNodeBase
    {
        public InvocationType Type;

        public HybType LeftType;
        public string RightName;

        public SSMethodInfo Method;
    }
    internal enum InvocationType
    {
        VarInvoke,
        LocalMethod,
        RemoteStaticMethod,
        RemoteMethod,
        PredefinedTypeStatic,
        ExpressionMethod,
        ExtensionMethod,

        Thiscall,
        Boundcall
    }
}

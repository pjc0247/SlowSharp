using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal struct OptPrefixUnary : OptNodeBase
    {
        public string operandId;
        public bool isPrimitiveIncOrDec;
    }
    internal struct OptPostfixUnary : OptNodeBase
    {
        public string operandId;
        public bool isPrimitiveIncOrDec;
    }
}

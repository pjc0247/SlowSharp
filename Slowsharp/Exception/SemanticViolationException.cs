using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class SemanticViolationException : SSRuntimeException
    {
        public SemanticViolationException(string msg) :
            base(msg)
        {
        }
    }
}

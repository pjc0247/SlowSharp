using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    /// <summary>
    /// Reperents syntax violations.
    /// Can be thrown both parse & runtime.
    /// </summary>
    public class SemanticViolationException : SSRuntimeException
    {
        public SemanticViolationException(string msg) :
            base(msg)
        {
        }
    }
}

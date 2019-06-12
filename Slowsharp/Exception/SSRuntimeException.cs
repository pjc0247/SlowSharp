using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    /// <summary>
    /// A baseclass of SlowSharp runtime exceptions.
    /// </summary>
    public class SSRuntimeException : Exception
    {
        public SSRuntimeException()
        {
        }
        public SSRuntimeException(string msg) :
            base(msg)
        {
        }
    }
}

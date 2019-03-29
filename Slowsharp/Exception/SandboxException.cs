using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class SandboxException : SSRuntimeException
    {
        public SandboxException(string msg) :
            base(msg)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class WrappedException : SSRuntimeException
    {
        public HybInstance exception { get; }

        public WrappedException(HybInstance exception)
        {
            this.exception = exception;
        }
    }
}

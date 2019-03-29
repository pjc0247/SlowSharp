using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class TimeoutException : SSRuntimeException
    {
        public TimeoutException() :
            base ("Execution has timed out")
        {
        }
    }
}

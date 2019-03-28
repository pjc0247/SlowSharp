using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class NoSuchMethodException : SSRuntimeException
    {
        public NoSuchMethodException(string calleeId, string id) :
            base($"{calleeId} does not contains method `{id}`.")
        {
        }
    }
}

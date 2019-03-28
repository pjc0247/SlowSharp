using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class NoSuchMemberException : SSRuntimeException
    {
        public NoSuchMemberException(string id) :
            base($"{id}")
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class RunConfig
    {
        public static RunConfig Default => new RunConfig();

        public IAccessFilter AccessControl { get; set; }
        public int Timeout { get; set; } = int.MaxValue;

        public RunConfig()
        {
            AccessControl = new DefaultAccessControl();
        }
    }
}

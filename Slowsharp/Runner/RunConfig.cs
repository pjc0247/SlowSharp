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

        public AccessControl accessControl { get; set; }
        public int timeout { get; set; } = int.MaxValue;

        public RunConfig()
        {
            accessControl = new AccessControl();
        }
    }
}

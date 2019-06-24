using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Slowsharp
{
    public sealed partial class Runner
    {
        private Dictionary<MethodInfo, MethodInfo> traps = new Dictionary<MethodInfo, MethodInfo>();

        public void Trap(MethodInfo target, MethodInfo replace)
        {
            traps[target] = replace;
        }
        public void Untrap(MethodInfo target)
        {
            traps.Remove(target);
        }
    }
}

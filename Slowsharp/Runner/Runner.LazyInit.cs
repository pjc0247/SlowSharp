using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public partial class Runner
    {
        private List<Action> initializers = new List<Action>();
        private bool initialized = false;

        public void AddLazyInitializer(Action callback)
        {
            initializers.Add(callback);
        }
        public void RunLazyInitializers()
        {
            if (initialized) return;
            initialized = true;

            foreach (var cb in initializers)
                cb();
        }
    }
}

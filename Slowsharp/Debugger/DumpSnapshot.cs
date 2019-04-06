using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class DumpSnapshot
    {
        public HybInstance _this { get; }

        private Runner runner { get; }
        private Dictionary<string, HybInstance> locals { get; }

        public DumpSnapshot(Runner runner)
        {
            this.runner = runner;
            this._this = runner.ctx._this;
            this.locals = runner.vars.Flatten();
        }
    }
}

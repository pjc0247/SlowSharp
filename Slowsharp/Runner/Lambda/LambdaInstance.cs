using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class LambdaInstance
    {
        public VarCapture capture;

        public HybInstance Create(Runner runner)
        {
            var lambda = new LambdaInstance(runner);
            return new HybInstance(new HybType(typeof(LambdaInstance)), lambda);
        }

        public LambdaInstance(Runner runner)
        {
            this.capture = new VarCapture(runner.vars);
        }
    }
}

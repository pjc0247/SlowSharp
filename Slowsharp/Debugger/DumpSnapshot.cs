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
        public Dictionary<string, HybInstance> locals { get; }
        public string methodSrc { get; }
        public int breakLine { get; }
        public CallStackFrame[] callStack { get; }

        private Runner runner { get; }

        public DumpSnapshot(Runner runner)
        {
            this.runner = runner;
            this._this = runner.ctx._this;
            this.locals = runner.vars.Flatten();
            this.methodSrc = runner.ctx.method.declaration.ToString();

            var method = runner.ctx.method;
            var methodLine = method.declaration.GetLocation().GetLineSpan().StartLinePosition.Line;
            var nodeLine = runner.ctx.lastNode.GetLocation().GetLineSpan().StartLinePosition.Line;
            breakLine = nodeLine - methodLine;

            var frames = new List<CallStackFrame>();
            foreach (var m in runner.ctx.methodStack)
            {
                frames.Add(new CallStackFrame() {
                    signature = m.signature
                });  
            }
            this.callStack = frames.ToArray();
        }
    }
}

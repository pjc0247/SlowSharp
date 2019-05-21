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
            this._this = runner.Ctx._this;
            this.locals = runner.Vars.Flatten();
            this.methodSrc = runner.Ctx.Method.Declaration.ToString();

            var method = runner.Ctx.Method;
            var methodLine = method.Declaration.GetLocation().GetLineSpan().StartLinePosition.Line;
            var nodeLine = runner.Ctx.LastNode.GetLocation().GetLineSpan().StartLinePosition.Line;
            breakLine = nodeLine - methodLine;

            var frames = new List<CallStackFrame>();
            foreach (var m in runner.Ctx.Callstack)
            {
                frames.Add(new CallStackFrame() {
                    signature = m.Method.Signature
                });  
            }
            this.callStack = frames.ToArray();
        }
    }
}

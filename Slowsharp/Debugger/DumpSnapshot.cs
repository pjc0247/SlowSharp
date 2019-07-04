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
        public Dictionary<string, HybInstance> Locals { get; }
        public string MethodSrc { get; }
        public int BreakLine { get; }
        public CallStackFrame[] Callstack { get; }

        private Runner Runner { get; }

        public DumpSnapshot(Runner runner)
        {
            this.Runner = runner;
            this._this = runner.Ctx._this;
            this.Locals = runner.Vars.Flatten();
            this.MethodSrc = runner.Ctx.Method.Declaration.ToString();

            var method = runner.Ctx.Method;
            var methodLine = method.Declaration.GetLocation().GetLineSpan().StartLinePosition.Line;
            var nodeLine = runner.Ctx.LastNode.GetLocation().GetLineSpan().StartLinePosition.Line;
            BreakLine = nodeLine - methodLine;

            var frames = new List<CallStackFrame>();
            foreach (var m in runner.Ctx.Callstack)
            {
                frames.Add(new CallStackFrame() {
                    signature = m.Method.Signature
                });  
            }
            this.Callstack = frames.ToArray();
        }
    }
}

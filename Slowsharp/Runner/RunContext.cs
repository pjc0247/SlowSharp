using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal class RunContext
    {
        public RunConfig Config { get; }
        public Dictionary<string, Class> Types { get; }

        public HybInstance _this { get; set; }
        public HybInstance _bound { get; set; }
        public SSMethodInfo Method { get; private set; }
        public SyntaxNode LastNode { get; set; }
        public Stack<CallFrame> Callstack { get; private set; }

        private DateTime startsAt;

        public RunContext(RunConfig config)
        {
            this.Config = config;

            this.Types = new Dictionary<string, Class>();
            this.Callstack = new Stack<CallFrame>();

            // This prevents bug
            Reset();
        }
        public void Reset()
        {
            startsAt = DateTime.Now;
        }
        public bool IsExpird() => (DateTime.Now - startsAt).TotalMilliseconds >= Config.Timeout;

        public void PushMethod(SSMethodInfo methodInfo)
        {
            Callstack.Push(new CallFrame()
            {
                _this = _this,
                Method = Method
            });
            Method = methodInfo;
        }
        public void PopMethod()
        {
            var prevCallframe = Callstack.Pop();
            _this = prevCallframe._this;
            Method = prevCallframe.Method;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal class RunContext
    {
        public RunConfig config { get; }
        public Dictionary<string, Class> types { get; }

        public HybInstance _this { get; set; }
        public SSMethodInfo method { get; private set; }

        private DateTime startsAt;
        private Stack<SSMethodInfo> methodStack;

        public RunContext(RunConfig config)
        {
            this.config = config;

            this.types = new Dictionary<string, Class>();
            this.methodStack = new Stack<SSMethodInfo>();

            // This prevents bug
            Reset();
        }
        public void Reset()
        {
            startsAt = DateTime.Now;
        }
        public bool IsExpird() => (DateTime.Now - startsAt).TotalMilliseconds >= config.timeout;

        public void PushMethod(SSMethodInfo methodInfo)
        {
            methodStack.Push(methodInfo);
            method = methodInfo;
        }
    }
}

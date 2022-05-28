using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public class SSEnumerator : IEnumerator<object>, IEnumerator
    {
        /// <summary>
        /// Program counter that indicates next execution point
        /// </summary>
        private int pc = 0;
        private BlockSyntax block;
        private VarFrame vf;
        private Runner runner;

        public  SSInterpretMethodInfo Method;

        public object Current => runner.Ret.Unwrap();

        internal SSEnumerator(Runner runner, BlockSyntax node, VarFrame vf)
        {
            this.runner = runner;
            this.block = node;
            this.vf = vf;
        }

        public void Dispose()
        {
            // releases gc refs just in case
            block = null;
            vf = null;
            runner = null;
        }
        public bool MoveNext()
        {
            if (pc == -1)
                return false;

            runner.Ctx.PushMethod(Method);
            pc = runner.RunBlock(block, vf, pc);
            runner.Ctx.PopMethod();

            // -1 means EndOfMethod
            return pc == -1 ? false : true;
        }
        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}

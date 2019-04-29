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
        public object Current => throw new NotImplementedException();

        private int pc = 0;
        private BlockSyntax block;
        private VarFrame vf;
        private Runner runner;

        internal SSEnumerator(Runner runner, BlockSyntax node, VarFrame vf)
        {
            this.runner = runner;
            this.block = node;
            this.vf = vf;
        }

        public void Dispose()
        {
        }
        public bool MoveNext()
        {
            runner.RunBlock(block, vf, pc);
        }
        public void Reset()
        {
        }
    }
}

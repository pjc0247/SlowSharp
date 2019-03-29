using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public partial class Runner
    {
        private void RunTry(TryStatementSyntax node)
        {
            catches.Push(new CatchFrame(this, node));
            RunBlock(node.Block);
            catches.Pop();

            if (node.Finally != null)
                Run(node.Finally.Block);
        }
    }
}

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
            bool hasCatches = node.Catches.Count > 0;

            if (hasCatches)
                Catches.Push(new CatchFrame(this, node));
            try
            {
                RunBlock(node.Block);
            }
            finally
            {
                if (node.Finally != null)
                    Run(node.Finally.Block);
            }
            if (hasCatches)
                Catches.Pop();
        }
    }
}

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
        private void RunReturn(ReturnStatementSyntax node)
        {
            if (node.Expression != null)
                ret = RunExpression(node.Expression);

            halt = HaltType.Return;
        }
    }
}

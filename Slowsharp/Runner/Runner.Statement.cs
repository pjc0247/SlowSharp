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
        private void RunThrow(ThrowStatementSyntax node)
        {
            var ex = RunExpression(node.Expression);

            if (ex.isCompiledType)
            {
                if (ex.innerObject is Exception e)
                    throw e;
                throw new SemanticViolationException($"Exception must be derived from System.Exception."); ;
            }
            throw new WrappedException(ex);
        }
        private void RunReturn(ReturnStatementSyntax node)
        {
            if (node.Expression != null)
                ret = RunExpression(node.Expression);

            halt = HaltType.Return;
        }
    }
}

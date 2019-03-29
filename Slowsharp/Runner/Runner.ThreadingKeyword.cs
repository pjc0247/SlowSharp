using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public partial class Runner
    {
        private void RunLock(LockStatementSyntax node)
        {
            var obj = RunExpression(node.Expression);

            if (obj == null || obj.IsNull())
                throw new NullReferenceException($"lock(value) cannot be a null");

            object lockObj = obj;
            if (obj.isCompiledType)
                lockObj = obj.Unwrap();

            Monitor.Enter(lockObj);
            try
            {
                Run(node.Statement);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        private HybInstance RunAwait(AwaitExpressionSyntax node)
        {
            throw new NotImplementedException();
        }
    }
}

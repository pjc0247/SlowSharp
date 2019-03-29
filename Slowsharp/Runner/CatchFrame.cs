using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal class CatchFrame
    {
        private Runner runner;
        private TryStatementSyntax tryNode;

        public CatchFrame(Runner runner, TryStatementSyntax node)
        {
            this.runner = runner;
            this.tryNode = node;
        }

        public bool RunCatch(Exception e)
        {
            foreach (var c in tryNode.Catches)
            {
                var type = $"{c.Declaration.Type}";
                var rt = runner.resolver.GetType(type);

                if (rt == null)
                    continue;
                if (e.GetType().IsSubclassOf(rt))
                {
                    var vf = new VarFrame(runner.vars);
                    vf.SetValue($"{c.Declaration.Identifier}", HybInstance.Object(e));
                    runner.RunBlock(c.Block, vf);
                    return true;
                }
            }

            return false;
        }
    }
}

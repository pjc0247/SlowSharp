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
        private Runner Runner;
        private TryStatementSyntax TryNode;

        public CatchFrame(Runner runner, TryStatementSyntax node)
        {
            this.Runner = runner;
            this.TryNode = node;
        }

        public bool RunCatch(Exception e)
        {
            foreach (var c in TryNode.Catches)
            {
                var type = $"{c.Declaration.Type}";
                var rt = Runner.Resolver.GetType(type);

                if (rt == null)
                    continue;
                if (e.GetType().IsSubclassOf(rt))
                {
                    var vf = new VarFrame(Runner.Vars);
                    vf.SetValue($"{c.Declaration.Identifier}", HybInstance.Object(e));
                    Runner.RunBlock(c.Block, vf);
                    return true;
                }
            }

            return false;
        }
    }
}

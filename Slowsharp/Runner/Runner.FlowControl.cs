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
        private void RunIf(IfStatementSyntax node)
        {
            var v = RunExpression(node.Condition);
            
            if (IsTrueOrEquivalent(v))
                Run(node.Statement);
            else if (node.Else != null)
                Run(node.Else.Statement);
        }
        private bool IsTrueOrEquivalent(object obj)
        {
            try
            {
                if (Convert.ToInt32(obj) == 0)
                    return false;
            }
            catch { }

            if (obj is bool && (bool)obj == false)
                return false;
            if (obj == null)
                return false;

            return true;
        }

        private void RunFor(ForStatementSyntax node)
        {
            vars = new VarFrame(vars);

            //foreach (var expr in node.Initializers)
            //RunExpression(expr);

            Run(node.Declaration);

            while (true)
            {
                var cond = RunExpression(node.Condition);
                if (IsTrueOrEquivalent(cond) == false)
                    break;

                Run(node.Statement);

                foreach (var expr in node.Incrementors)
                    RunExpression(expr);
            }

            vars = vars.parent;
        }
    }
}

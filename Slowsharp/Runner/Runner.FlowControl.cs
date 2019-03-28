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
        private bool IsTrueOrEquivalent(HybInstance obj)
        {
            if (obj == null) return false;

            try
            {
                if (obj.isCompiledType &&
                    Convert.ToInt32(obj.innerObject) == 0)
                    return false;
            }
            catch { }

            if (obj.Is<bool>() && obj.As<bool>() == false)
                return false;
            if (obj.As<object>() == null)
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
                if (node.Condition != null)
                {
                    var cond = RunExpression(node.Condition);
                    if (IsTrueOrEquivalent(cond) == false)
                        break;
                }

                Run(node.Statement);
                if (halt) break;

                foreach (var expr in node.Incrementors)
                    RunExpression(expr);
            }

            vars = vars.parent;
        }
    }
}

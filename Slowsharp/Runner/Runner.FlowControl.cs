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
        private void RunGoto(GotoStatementSyntax node)
        {
            var label = $"{node.Expression.GetText()}";
            var dst = Ctx.Method.Jumps
                .Where(x => x.Label == label)
                .FirstOrDefault();

            if (dst == null)
                throw new SemanticViolationException($"goto destination not found: {label}");

            // TODO: frame unwinding
            //   goto is very unstalbe in current implementation
            RunBlock((BlockSyntax)dst.Statement, Vars, dst.Pc);
        }

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
                if (obj.IsCompiledType &&
                    Convert.ToInt32(obj.InnerObject) == 0)
                    return false;
            }
            catch { }

            if (obj.Is<bool>() && obj.As<bool>() == false)
                return false;
            if (obj.As<object>() == null)
                return false;

            return true;
        }

        private void RunSwitch(SwitchStatementSyntax node)
        {
            var value = RunExpression(node.Expression);

            foreach (var section in node.Sections)
            {
                foreach (var label in section.Labels)
                {
                    if (label is DefaultSwitchLabelSyntax)
                    {
                        foreach (var statement in section.Statements)
                        {
                            Run(statement);

                            if (Halt != HaltType.None)
                                break;
                        }
                    }
                    else if (label is CaseSwitchLabelSyntax caseLabel)
                    {
                        var caseValue = RunExpression(caseLabel.Value);

                        if (MadMath.Eq(value, caseValue).As<bool>())
                        {
                            foreach (var statement in section.Statements)
                            {
                                Run(statement);

                                if (Halt != HaltType.None)
                                    break;
                            }
                        }
                    }

                    if (Halt != HaltType.None)
                        break;
                }

                if (Halt != HaltType.None)
                    break;
            }

            if (Halt == HaltType.Break)
                Halt = HaltType.None;
        }

        private void RunFor(ForStatementSyntax node)
        {
            Vars = new VarFrame(Vars);

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

                if (Halt == HaltType.Continue)
                    Halt = HaltType.None;
                if (Halt != HaltType.None) break;

                foreach (var expr in node.Incrementors)
                    RunExpression(expr);
            }

            if (Halt == HaltType.Break)
                Halt = HaltType.None;

            Vars = Vars.Parent;
        }

        private void RunForEach(ForEachStatementSyntax node)
        {
            var list = RunExpression(node.Expression);
            var e = list.GetEnumerator();

            Vars = new VarFrame(Vars);
            while (true)
            {
                if (e.MoveNext() == false)
                    break;
                
                Vars.SetValue($"{node.Identifier}", e.Current.Wrap());

                Run(node.Statement);

                if (Halt == HaltType.Continue || 
                    Halt == HaltType.YieldReturn)
                    Halt = HaltType.None;
                if (Halt != HaltType.None) break;
            }
            Vars = Vars.Parent;

            if (Halt == HaltType.Break)
                Halt = HaltType.None;
        }
        private void RunWhile(WhileStatementSyntax node)
        {
            while (true)
            {
                var cond = RunExpression(node.Condition);
                if (IsTrueOrEquivalent(cond) == false)
                    break;

                Run(node.Statement);

                if (Halt == HaltType.Continue)
                    Halt = HaltType.None;
                if (Halt != HaltType.None) break;
            }

            if (Halt == HaltType.Break)
                Halt = HaltType.None;
        }

        private void RunBreak(BreakStatementSyntax node)
        {
            Halt = HaltType.Break;
        }
        private void RunContinue(ContinueStatementSyntax node)
        {
            Halt = HaltType.Continue;
        }
    }
}

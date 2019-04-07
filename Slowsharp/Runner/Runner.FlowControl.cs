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
            var dst = ctx.method.jumps
                .Where(x => x.label == label)
                .FirstOrDefault();

            if (dst == null)
                throw new SemanticViolationException($"goto destination not found: {label}");

            // TODO: frame unwinding
            //   goto is very unstalbe in current implementation
            RunBlock((BlockSyntax)dst.statement, vars, dst.pc);
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

                            if (halt != HaltType.None)
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

                                if (halt != HaltType.None)
                                    break;
                            }
                        }
                    }

                    if (halt != HaltType.None)
                        break;
                }

                if (halt != HaltType.None)
                    break;
            }

            if (halt == HaltType.Break)
                halt = HaltType.None;
        }

        private void RunFor(ForStatementSyntax node)
        {
            vars = new VarFrame(vars);

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

                if (halt == HaltType.Continue)
                    halt = HaltType.None;
                if (halt != HaltType.None) break;

                foreach (var expr in node.Incrementors)
                    RunExpression(expr);
            }

            if (halt == HaltType.Break)
                halt = HaltType.None;

            vars = vars.parent;
        }

        private void RunForEach(ForEachStatementSyntax node)
        {
            var list = RunExpression(node.Expression);
            var e = list.GetEnumerator();

            vars = new VarFrame(vars);
            while (true)
            {
                if (e.MoveNext() == false)
                    break;
                
                vars.SetValue($"{node.Identifier}", e.Current.Wrap());

                Run(node.Statement);

                if (halt == HaltType.Continue)
                    halt = HaltType.None;
                if (halt != HaltType.None) break;
            }
            vars = vars.parent;

            if (halt == HaltType.Break)
                halt = HaltType.None;
        }
        private void RunWhile(WhileStatementSyntax node)
        {
            while (true)
            {
                var cond = RunExpression(node.Condition);
                if (IsTrueOrEquivalent(cond) == false)
                    break;

                Run(node.Statement);

                if (halt == HaltType.Continue)
                    halt = HaltType.None;
                if (halt != HaltType.None) break;
            }

            if (halt == HaltType.Break)
                halt = HaltType.None;
        }

        private void RunBreak(BreakStatementSyntax node)
        {
            halt = HaltType.Break;
        }
        private void RunContinue(ContinueStatementSyntax node)
        {
            halt = HaltType.Continue;
        }
    }
}

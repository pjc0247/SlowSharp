using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Slowsharp
{
    public partial class Runner
    {
        private HybInstance RunParenthesizedLambda(ParenthesizedLambdaExpressionSyntax node)
        {
            // Detects the lambda is `Func` or `Action`.
            var hasReturn = node.DescendantNodes()
                .Any(x => x is ReturnStatementSyntax);
            var ps = node.ParameterList.Parameters;
            var retType = TypeDeduction.GetReturnType(resolver, node.Body);

            MethodInfo converter = null;
            object body = null;

            // `Func`
            if (hasReturn)
            {
                converter = typeof(Runner)
                    .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                    .Where(x => x.Name == nameof(ConvertF))
                    .Where(x => x.GetGenericArguments().Length == ps.Count)
                    .First();

                var genericArgs = new Type[ps.Count + 1];
                for (int i = 0; i < ps.Count; i++) {
                    if (ps[i].Type == null)
                        throw new SemanticViolationException("Please provide a explicit type to all lambda parameters, this function is partialy implemented.");
                    genericArgs[i] = resolver
                        .GetType($"{ps[i].Type}")
                        .Unwrap();
                }
                genericArgs[genericArgs.Length - 1] = retType.Unwrap();
                converter = converter
                    .MakeGenericMethod(genericArgs);

                if (ps.Count == 0)
                {
                    body = new Func<object>(() => {
                        RunBlock(node.Body as BlockSyntax);
                        if (halt == HaltType.Return)
                            halt = HaltType.None;
                        return ret.Unwrap();
                    });
                }
                else if (ps.Count == 1)
                {
                    body = new Func<object, object>((a) => {
                        return FuncBody(ps, node.Body as BlockSyntax, a);
                    });
                }
                else if (ps.Count == 2)
                {
                    body = new Func<object, object, object>((a, b) => {
                        return FuncBody(ps, node.Body as BlockSyntax, a, b);
                    });
                }
                else if (ps.Count == 3)
                {
                    body = new Func<object, object, object, object>((a, b, c) => {
                        return FuncBody(ps, node.Body as BlockSyntax, a, b, c);
                    });
                }
                else if (ps.Count == 4)
                {
                    body = new Func<object, object, object, object, object>((a, b, c, d) => {
                        return FuncBody(ps, node.Body as BlockSyntax, a, b, c, d);
                    });
                }
                else if (ps.Count == 5)
                {
                    body = new Func<object, object, object, object, object, object>((a, b, c, d, e) => {
                        return FuncBody(ps, node.Body as BlockSyntax, a, b, c, d, e);
                    });
                }
                else if (ps.Count == 6)
                {
                    body = new Func<object, object, object, object, object, object, object>((a, b, c, d, e, f) => {
                        return FuncBody(ps, node.Body as BlockSyntax, a, b, c, d, e, f);
                    });
                }
                else if (ps.Count == 7)
                {
                    body = new Func<object, object, object, object, object, object, object, object>((a, b, c, d, e, f, g) => {
                        return FuncBody(ps, node.Body as BlockSyntax, a, b, c, d, e, f, g);
                    });
                }
            }
            // `Action`
            else
            {
                if (ps.Count == 0)
                {
                    body = new Action(() => {
                        RunBlock(node.Body as BlockSyntax);
                        if (halt == HaltType.Return)
                            halt = HaltType.None;
                    });
                }
                else if (ps.Count == 1)
                {
                    body = new Action<object>((a) => {
                        ActionBody(ps, node.Body as BlockSyntax, a);
                    });
                }
                else if (ps.Count == 2)
                {
                    body = new Action<object, object>((a, b) => {
                        ActionBody(ps, node.Body as BlockSyntax, a, b);
                    });
                }
                else if (ps.Count == 3)
                {
                    body = new Action<object, object, object>((a, b, c) => {
                        ActionBody(ps, node.Body as BlockSyntax, a, b, c);
                    });
                }
                else if (ps.Count == 4)
                {
                    body = new Action<object, object, object, object>((a, b, c, d) => {
                        ActionBody(ps, node.Body as BlockSyntax, a, b, c, d);
                    });
                }
                else if (ps.Count == 5)
                {
                    body = new Action<object, object, object, object, object>((a, b, c, d, e) => {
                        ActionBody(ps, node.Body as BlockSyntax, a, b, c, d, e);
                    });
                }
                else if (ps.Count == 6)
                {
                    body = new Action<object, object, object, object, object, object>((a, b, c, d, e, f) => {
                        ActionBody(ps, node.Body as BlockSyntax, a, b, c, d, e, f);
                    });
                }
                else if (ps.Count == 7)
                {
                    body = new Action<object, object, object, object, object, object, object>((a, b, c, d, e, f, g) => {
                        ActionBody(ps, node.Body as BlockSyntax, a, b, c, d, e, f, g);
                    });
                }
            }

            var convertedDelegate = converter.Invoke(
                null, new object[] { body });
            return new HybInstance(
                new HybType(convertedDelegate.GetType()), 
                convertedDelegate);
        }

        private void ActionBody(SeparatedSyntaxList<ParameterSyntax> ps, BlockSyntax body, params object[] args)
        {
            var vf = new VarFrame(vars);
            for (int i = 0; i < ps.Count; i++)
                vf.SetValue($"{ps[i].Identifier}", HybInstance.Object(args[i]));
            RunBlock(body as BlockSyntax, vf);
            if (halt == HaltType.Return)
                halt = HaltType.None;
        }
        private object FuncBody(SeparatedSyntaxList<ParameterSyntax> ps, BlockSyntax body, params object[] args)
        {
            var vf = new VarFrame(vars);
            for (int i = 0; i< ps.Count; i++) 
                vf.SetValue($"{ps[i].Identifier}", HybInstance.Object(args[i]));
            RunBlock(body as BlockSyntax, vf);
            if (halt == HaltType.Return)
                halt = HaltType.None;

            return ret.Unwrap();
        }

        private static Action ConvertA(Action func)
            => new Action(() => func());
        private static Action<T1> ConvertA<T1>(Action<object> func)
            => new Action<T1>((a) => func((T1)a));
        private static Action<T1, T2> ConvertA<T1, T2>(Action<object, object> func)
            => new Action<T1, T2>((a, b) => func((T1)a, (T2)b));
        private static Action<T1, T2, T3> ConvertA<T1, T2, T3>(Action<object, object, object> func)
            => new Action<T1, T2, T3>((a, b, c) => func((T1)a, (T2)b, (T3)c));
        private static Action<T1, T2, T3, T4> ConvertA<T1, T2, T3, T4>(Action<object, object, object, object> func)
            => new Action<T1, T2, T3, T4>((a, b, c, d) => func((T1)a, (T2)b, (T3)c, (T4)d));
        private static Action<T1, T2, T3, T4, T5> ConvertA<T1, T2, T3, T4, T5>(Action<object, object, object, object, object> func)
            => new Action<T1, T2, T3, T4, T5>((a, b, c, d, e) => func((T1)a, (T2)b, (T3)c, (T4)d, (T5)e));
        private static Action<T1, T2, T3, T4, T5, T6> ConvertA<T1, T2, T3, T4, T5, T6>(Action<object, object, object, object, object, object> func)
            => new Action<T1, T2, T3, T4, T5, T6>((a, b, c, d, e, f) => func((T1)a, (T2)b, (T3)c, (T4)d, (T5)e, (T6)f));
        private static Action<T1, T2, T3, T4, T5, T6, T7> ConvertA<T1, T2, T3, T4, T5, T6, T7>(Action<object, object, object, object, object, object, object> func)
            => new Action<T1, T2, T3, T4, T5, T6, T7>((a, b, c, d, e, f, g) => func((T1)a, (T2)b, (T3)c, (T4)d, (T5)e, (T6)f, (T7)g));

        private static Func<R> ConvertF<R>(Func<object> func)
            => new Func<R>(() => (R)func());
        private static Func<T1, R> ConvertF<T1, R>(Func<object, object> func)
            => new Func<T1, R>(a => (R)func((T1)a));
        private static Func<T1, T2, R> ConvertF<T1, T2, R>(Func<object, object, object> func)
            => new Func<T1, T2, R>((a,b) => (R)func((T1)a, (T2)b));
        private static Func<T1, T2, T3, R> ConvertF<T1, T2, T3, R>(Func<object, object, object, object> func)
            => new Func<T1, T2, T3, R>((a,b,c) => (R)func((T1)a, (T2)b, (T3)c));
        private static Func<T1, T2, T3, T4, R> ConvertF<T1, T2, T3, T4, R>(Func<object, object, object, object, object> func)
            => new Func<T1, T2, T3, T4, R>((a, b, c, d) => (R)func((T1)a, (T2)b, (T3)c, (T4)d));
        private static Func<T1, T2, T3, T4, T5, R> ConvertF<T1, T2, T3, T4, T5, R>(Func<object, object, object, object, object, object> func)
            => new Func<T1, T2, T3, T4, T5, R>((a, b, c, d, e) => (R)func((T1)a, (T2)b, (T3)c, (T4)d, (T5)e));
        private static Func<T1, T2, T3, T4, T5, T6, R> ConvertF<T1, T2, T3, T4, T5, T6, R>(Func<object, object, object, object, object, object, object> func)
            => new Func<T1, T2, T3, T4, T5, T6, R>((a, b, c, d, e, f) => (R)func((T1)a, (T2)b, (T3)c, (T4)d, (T5)e, (T6) f));
        private static Func<T1, T2, T3, T4, T5, T6, T7, R> ConvertF<T1, T2, T3, T4, T5, T6, T7, R>(Func<object, object, object, object, object, object, object, object> func)
            => new Func<T1, T2, T3, T4, T5, T6, T7, R>((a, b, c, d, e, f, g) => (R)func((T1)a, (T2)b, (T3)c, (T4)d, (T5)e, (T6)f, (T7) g));
    }
}

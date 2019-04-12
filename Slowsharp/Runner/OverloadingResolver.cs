using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class OverloadingResolver
    {
        public static SSMethodInfo FindMethodWithArguments(
            TypeResolver resolver, SSMethodInfo[] members, 
            HybType[] implicitGenercArgs,
            ref HybInstance[] args)
        {
            var originalArgs = (HybInstance[])args.Clone();

            foreach (var member in members)
            {
                if (member.target.isCompiled)
                {
                    args = originalArgs;

                    var genericBound = new Dictionary<string, Type>();
                    var genericArgs = new List<HybType>(implicitGenercArgs);
                    var method = member.target.compiledMethod;
                    var ps = method.GetParameters();

                    if (args.Length > ps.Length)
                        continue;

                    bool match = true;
                    for (int i = 0; i < ps.Length; i++)
                    {
                        var p = ps[i].ParameterType;

                        if (args.Length <= i)
                        {
                            if (ps[i].IsOptional == false)
                            {
                                match = false;
                                break;
                            }
                            continue;
                        }

                        if (p.IsByRef)
                            p = p.GetElementType();

                        if (args[i] == null || args[i].IsNull())
                        {
                            if (p.IsValueType)
                            {
                                match = false;
                                break;
                            }
                            continue;
                        }

                        var argType = args[i].GetHybType();
                        if (!p.IsAssignableFromEx(argType, genericBound))
                        {
                            // Second change,
                            // Check whether parent can be assignable
                            if (args[i].isVirtualDerived &&
                                p.IsAssignableFromEx(args[i].parent.GetHybType(), genericBound))
                            {
                                args[i] = args[i].parent;
                            }
                            else
                            {
                                match = false;
                                break;
                            }
                        }

                        if (p.IsGenericType || p.IsGenericTypeDefinition)
                        {
                            /*
                            if (argType.isCompiledType)
                            {
                                genericArgs.AddRange(
                                    genericBound.Select(x => new HybType(x)));
                            }
                            else
                                genericArgs.Add(new HybType(typeof(HybInstance)));
                                */
                        }
                    }
                    if (match == false)
                        continue;

                    var methodGenericArgs = member.GetGenericArgumentsFromDefinition();
                    if (methodGenericArgs.Length > 0)
                    {
                        foreach (var arg in methodGenericArgs)
                        {
                            if (genericBound.ContainsKey(arg.Name) == false)
                                throw new SemanticViolationException($"Insufficient generic arguments for `{member.id}`");

                            genericArgs.Add(new HybType(genericBound[arg.Name]));
                        }

                        return member.MakeGenericMethod(genericArgs.ToArray());
                    }
                    return member;
                }
                else
                {
                    var ps = member.target.interpretMethod.ParameterList.Parameters;

                    if (member.isVaArg == false &&
                        args.Length > ps.Count)
                        continue;

                    var match = true;
                    var count = 0;
                    foreach (var p in ps)
                    {
                        var paramType = resolver.GetType($"{p.Type}");

                        if (p.Modifiers.IsParams())
                            break;
                        if (args.Length <= count)
                        {
                            match = false;
                            break;
                        }

                        var argType = args[count++].GetHybType();

                        if (paramType.IsAssignableFrom(argType) == false)
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match == false)
                        continue;

                    return member;
                }
            }

            return null;
        }
    }
}

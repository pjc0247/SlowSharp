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
            TypeResolver resolver, SSMethodInfo[] members, HybInstance[] args)
        {
            foreach (var member in members)
            {
                if (member.target.isCompiled)
                {
                    var method = member.target.compiledMethod;
                    var ps = method.GetParameters();

                    if (args.Length != ps.Length)
                        continue;

                    bool skip = false;
                    for (int i = 0; i < ps.Length; i++)
                    {
                        if (args[i] == null)
                        {
                            if (ps[i].ParameterType.IsValueType)
                            {
                                skip = true;
                                break;
                            }
                            continue;
                        }
                        if (!ps[i].ParameterType.IsAssignableFrom(args[i].GetHybType().compiledType))
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (skip) continue;

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

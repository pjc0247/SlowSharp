using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class OverloadingResolver
    {
        public static SSMethodInfo FindMethodWithArguments(SSMethodInfo[] members, HybInstance[] args)
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

                    if (args.Length != ps.Count)
                        continue;

                    foreach (var p in ps)
                    {
                        Console.WriteLine(p.Type);
                        //p.Type
                    }

                    return member;
                }
            }

            return null;
        }
    }
}

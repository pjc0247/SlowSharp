using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class IdLookup
    {
        private List<string> Usings = new List<string>();
        private Assembly[] References;

        public IdLookup(Assembly[] references)
        {
            this.References = references;
        }

        public void Add(string ns)
        {
            Usings.Add(ns);
        }

        public MemberInfo[] GetMembers(string typename, string membername)
        {
            foreach (var asm in References)
            {
                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == typename)
                    {
                        return type.GetMember(membername);
                    }
                }
            }

            return new MemberInfo[] { };
        }
    }
}

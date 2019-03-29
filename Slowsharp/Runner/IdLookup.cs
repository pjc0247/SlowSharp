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
        private List<string> usings = new List<string>();
        private Assembly[] references;

        public IdLookup(Assembly[] references)
        {
            this.references = references;
        }

        public void Add(string ns)
        {
            usings.Add(ns);
        }

        public MemberInfo[] GetMembers(string typename, string membername)
        {
            foreach (var asm in references)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class SSMemberInfo
    {
        public string id;
        public bool isStatic;

        public AccessModifier accessModifier;

        internal Class declaringClass;
    }
}

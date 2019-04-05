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
        public string signature;
        public bool isStatic;

        public AccessModifier accessModifier;

        public HybType declaringType;
        internal Class declaringClass;
    }
}

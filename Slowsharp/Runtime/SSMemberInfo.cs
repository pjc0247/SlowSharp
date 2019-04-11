using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public enum SSMemberOrigin
    {
        /// <summary>
        /// Member is declared in compiled assembly
        /// </summary>
        CompiledAssembly,

        /// <summary>
        /// Member is declared in script
        /// </summary>
        InterpretScript
    }

    public class SSMemberInfo
    {
        public string id;
        public string signature;
        public bool isStatic;

        public SSMemberOrigin origin { get; protected set; }

        public AccessModifier accessModifier;

        public HybType declaringType;
        internal Class declaringClass;
    }
}

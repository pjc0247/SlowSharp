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
        public string Id;
        public string Signature;
        public bool IsStatic;

        public SSMemberOrigin Origin { get; protected set; }

        public AccessModifier AccessModifier;

        public HybType DeclaringType;
        internal Class DeclaringClass;
    }
}

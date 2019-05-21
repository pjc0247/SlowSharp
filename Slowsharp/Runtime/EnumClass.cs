using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal class EnumClass : Class
    {
        private int NextValue = 0;

        public EnumClass(Runner runner, string id)
            : base(runner, id)
        {
        }

        public int AddMember(EnumMemberDeclarationSyntax node)
        {
            AddField(new SSInterpretFieldInfo(this)
            {
                Id = node.Identifier.Text,
                AccessModifier = AccessModifier.Public,
                IsStatic = true
            });

            var value = NextValue;

            if (node.EqualsValue != null)
                value = Runner.RunExpression(node.EqualsValue.Value).As<int>();
            NextValue = value + 1;

            return value;
        }
    }
}

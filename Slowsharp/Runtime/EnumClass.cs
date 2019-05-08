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
        private int nextValue = 0;

        public EnumClass(Runner runner, string id)
            : base(runner, id)
        {
        }

        public int AddMember(EnumMemberDeclarationSyntax node)
        {
            AddField(new SSInterpretFieldInfo(this)
            {
                id = node.Identifier.Text,
                accessModifier = AccessModifier.Public,
                isStatic = true
            });

            var value = nextValue;

            if (node.EqualsValue != null)
                value = runner.RunExpression(node.EqualsValue.Value).As<int>();
            nextValue = value + 1;

            return value;
        }
    }
}

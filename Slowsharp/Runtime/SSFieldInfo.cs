using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    /// <summary>
    /// SlowSharpFieldInfo
    /// </summary>
    public class SSFieldInfo : SSMemberInfo
    {
        public HybType fieldType;

        public VariableDeclaratorSyntax declartor;
        public FieldDeclarationSyntax field;
    }
}

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
    internal class SSFieldInfo : SSMemberInfo
    {
        public string id;
        public VariableDeclaratorSyntax declartor;
        public FieldDeclarationSyntax field;
    }
}

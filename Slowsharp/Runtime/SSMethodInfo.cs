using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal class SSMethodInfo : SSMemberInfo
    {
        public Invokable target;

        public BaseMethodDeclarationSyntax method;
    }
}

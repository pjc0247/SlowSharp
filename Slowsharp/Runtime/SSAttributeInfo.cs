using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public class SSAttributeInfo
    {
        public string Id;

        public SSAttributeInfo(AttributeSyntax node)
        {
            this.Id = $"{node.Name}";
        }
    }
}

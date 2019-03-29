using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal class JumpDestination
    {
        public string label;

        public StatementSyntax statement;
        public int pc;
        public int frameDepth;
    }
}

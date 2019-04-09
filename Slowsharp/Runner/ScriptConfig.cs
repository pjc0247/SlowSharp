using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class ScriptConfig
    {
        public static ScriptConfig Default => new ScriptConfig();

        public string[] DefaultUsings { get; set; } = new string[] { };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    /// <summary>
    /// 
    /// </summary>
    public class ScriptConfig
    {
        /// <summary>
        /// Creates a new config with default values
        /// </summary>
        public static ScriptConfig Default => new ScriptConfig();

        /// <summary>
        /// List of namespaces
        /// </summary>
        public string[] DefaultUsings { get; set; } = new string[] { };

        public Type[] PrewarmTypes { get; set; } = new Type[] { };
    }
}

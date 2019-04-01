using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal enum HaltType
    {
        /// <summary>
        /// Keep running
        /// </summary>
        None,

        /// <summary>
        /// Skip until loop ends
        /// </summary>
        Break,

        Continue,
        /// <summary>
        /// Skip until function ends
        /// </summary>
        Return,

        /// <summary>
        /// Stop running by API or Timeout.
        /// Should be stopped immediatly
        /// </summary>
        ForceQuit
    }
}

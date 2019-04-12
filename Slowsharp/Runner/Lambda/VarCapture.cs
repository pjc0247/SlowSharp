using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class VarCapture : VarFrame
    {
        public VarCapture(VarFrame parent) :
            base(parent)
        {
        }
    }
}

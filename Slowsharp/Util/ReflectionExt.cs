using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal static class ReflectionExt
    {
        /// <summary>
        /// Retrives types from assembly, without ReflectionTypeLoadException.
        /// </summary>
        public static Type[] GetTypesSafe(this Assembly _this)
        {
            try
            {
                return _this.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                return new Type[] { };
            }
        }
    }
}

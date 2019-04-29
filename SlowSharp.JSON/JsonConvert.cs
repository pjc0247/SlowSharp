using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp.JSON
{
    public class JsonConvert
    {
        public static string SerializeObject(HybInstance obj)
        {
            var sb = new StringBuilder();
            _SerializeObject(sb, obj);
            return sb.ToString();
        }
        private static void _SerializeObject(StringBuilder sb, HybInstance obj)
        {
            if (obj == null)
            {
                sb.Append("null");
                return;
            }

            var type = obj.GetHybType();
            if (type.isPrimitive)
            {
                if (type.compiledType == typeof(string))
                    sb.Append("\"");
                sb.Append(obj.innerObject.ToString());
                if (type.compiledType == typeof(string))
                    sb.Append("\"");
                return;
            }

            if (type.isArray)
            {
                sb.Append("[");

                sb.Append("]");
            }
        }
    }
}

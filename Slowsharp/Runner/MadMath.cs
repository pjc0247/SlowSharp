using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class MadMath
    {
        public static HybInstance Op(HybInstance a, HybInstance b, char op)
        {
            if (op == '+') return Add(a, b);
            if (op == '-') return Sub(a, b);
            if (op == '*') return Mul(a, b);
            if (op == '/') return Div(a, b);

            throw new ArgumentException($"Unrecognized operator: '{op}'.");
        }

        public static HybInstance Add(HybInstance a, HybInstance b)
        {
            if (a.isCompiledType && b.isCompiledType)
                return HybInstance.Object(_Add(a.innerObject, b.innerObject));

            throw new NotImplementedException();
        }
        public static HybInstance Sub(HybInstance a, HybInstance b)
        {
            if (a.isCompiledType && b.isCompiledType)
                return HybInstance.Object(_Sub(a.innerObject, b.innerObject));

            throw new NotImplementedException();
        }
        public static HybInstance Mul(HybInstance a, HybInstance b)
        {
            if (a.isCompiledType && b.isCompiledType)
                return HybInstance.Object(_Mul(a.innerObject, b.innerObject));

            throw new NotImplementedException();
        }
        public static HybInstance Div(HybInstance a, HybInstance b)
        {
            if (a.isCompiledType && b.isCompiledType)
                return HybInstance.Object(_Div(a.innerObject, b.innerObject));

            throw new NotImplementedException();
        }

        private static dynamic _Add(dynamic a, dynamic b ) => a + b;
        private static dynamic _Sub(dynamic a, dynamic b)  => a - b;
        private static dynamic _Mul(dynamic a, dynamic b) => a * b;
        private static dynamic _Div(dynamic a, dynamic b) => a / b;

        private static HybInstance AddInt32(HybInstance a, HybInstance b)
        {
            Int32 ia = a.As<Int32>();

            if (b.Is<Int16>()) return HybInstance.Int(ia + b.As<Int32>());
            if (b.Is<Int32>()) return HybInstance.Int(ia + b.As<Int32>());
            if (b.Is<Int64>()) return HybInstance.Int64(ia + b.As<Int64>());
            if (b.Is<float>()) return HybInstance.Float(ia + b.As<float>());

            throw new SemanticViolationException($"");
        }
    }
}

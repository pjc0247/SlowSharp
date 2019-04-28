using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal static class MadMath
    {
        public static HybInstance PrefixUnary(HybInstance a, string op)
        {
            if (op == "!") return PrefixNot(a);
            if (op == "-") return PrefixMinus(a);
            if (op == "+") return PrefixPlus(a);

            throw new ArgumentException($"Unrecognized operator: '{op}'.");
        }
        public static HybInstance PostfixUnary(HybInstance a, string op)
        {
            if (op == "++") return PostfixInc(a);
            if (op == "--") return PostfixDec(a);

            throw new ArgumentException($"Unrecognized operator: '{op}'.");
        }
        public static HybInstance Op(HybInstance a, HybInstance b, string op)
        {
            if (op == "+") return Add(a, b);
            if (op == "-") return Sub(a, b);
            if (op == "*") return Mul(a, b);
            if (op == "/") return Div(a, b);
            if (op == ">") return G(a, b);
            if (op == ">=") return GE(a, b);
            if (op == "<") return L(a, b);
            if (op == "<=") return LE(a, b);
            if (op == "==") return Eq(a, b);
            if (op == "!=") return Neq(a, b);
            if (op == "||") return Or(a, b);
            if (op == "&&") return And(a, b);
            if (op == "&") return BitwiseAnd(a, b);
            if (op == "|") return BitwiseAnd(a, b);

            throw new ArgumentException($"Unrecognized operator: '{op}'.");
        }

        public static HybInstance PrefixNot(HybInstance a)
        {
            if (a.isCompiledType)
            {
                if (a.Is<bool>())
                {
                    return a.As<bool>() ? 
                        HybInstanceCache.False : 
                        HybInstanceCache.True;
                }
            }

            var plusMethod = GetUnaryPlusMethod(a);
            if (plusMethod != null)
                return plusMethod.target.Invoke(null, new HybInstance[] { a });

            throw new NotImplementedException();
        }

        public static HybInstance PrefixMinus(HybInstance a)
        {
            if (a.isCompiledType)
            {
                if (a.GetHybType().isValueType)
                {
                    if (a.Is<Decimal>()) return HybInstance.Decimal(-a.As<Decimal>());
                    if (a.Is<Double>()) return HybInstance.Double(-a.As<Double>());
                    if (a.Is<Single>()) return HybInstance.Float(-a.As<Single>());
                    if (a.Is<Int64>()) return HybInstance.Int64(-a.As<Int64>());
                    if (a.Is<Int32>()) return HybInstance.Int(-a.As<Int32>());
                    if (a.Is<short>()) return HybInstance.Short(-a.As<short>());
                    if (a.Is<sbyte>()) return HybInstance.Byte(-a.As<sbyte>());
                }
            }

            var negationMethod = GetUnaryNegationMethod(a);
            if (negationMethod != null)
                return negationMethod.target.Invoke(null, new HybInstance[] { a });

            throw new NotImplementedException();
        }

        public static HybInstance PrefixPlus(HybInstance a)
        {
            if (a.isCompiledType)
                return HybInstance.Object(_PrefixPlus(a.innerObject));
            throw new NotImplementedException();
        }
        private static dynamic _PrefixPlus(dynamic a) => +a;

        public static HybInstance PostfixInc(HybInstance a)
        {
            if (a.isCompiledType)
            {
                if (a.GetHybType().isValueType)
                {
                    if (a.Is<Int64>()) return HybInstance.Int64(a.As<Int64>() + 1);
                    if (a.Is<Int32>()) return HybInstance.Int(a.As<Int32>() + 1);
                    if (a.Is<short>()) return HybInstance.Short(a.As<short>() + 1);
                    if (a.Is<byte>()) return HybInstance.Byte(a.As<byte>() + 1);
                }
            }

            var incMethod = GetIncMethod(a);
            if (incMethod != null)
                return incMethod.target.Invoke(null, new HybInstance[] { a });

            throw new NotImplementedException();
        }
        public static HybInstance PostfixDec(HybInstance a)
        {
            if (a.isCompiledType)
            {
                if (a.GetHybType().isValueType)
                {
                    if (a.Is<Int64>()) return HybInstance.Int64(a.As<Int64>() - 1);
                    if (a.Is<Int32>()) return HybInstance.Int(a.As<Int32>() - 1);
                    if (a.Is<short>()) return HybInstance.Short(a.As<short>() - 1);
                    if (a.Is<byte>()) return HybInstance.Byte(a.As<byte>() - 1);
                }
            }

            var decMethod = GetDecMethod(a);
            if (decMethod != null)
                return decMethod.target.Invoke(null, new HybInstance[] { a });

            throw new NotImplementedException();
        }

        public static HybInstance Add(HybInstance a, HybInstance b)
        {
            if (a.IsNull())
                throw new NullReferenceException(a.id);

            if (a.GetHybType().isPrimitive)
            {
                (a, b) = Promote(a, b);

                if (a.Is<Int32>()) return HybInstance.Int(a.As<Int32>() + b.As<Int32>());
                if (a.Is<Int64>()) return HybInstance.Int64(a.As<Int64>() + b.As<Int64>());
                if (a.Is<Single>()) return HybInstance.Float(a.As<Single>() + b.As<Single>());
                if (a.Is<Double>()) return HybInstance.Double(a.As<Double>() + b.As<Double>());
            }
            else if (a.Is<String>())
                return HybInstance.String(a.As<String>() + b.As<String>());

            var addMethod = GetAddMethod(a);
            if (addMethod != null)
                return addMethod.target.Invoke(null, new HybInstance[] { a, b });

            throw new NotImplementedException();
        }
        public static HybInstance Sub(HybInstance a, HybInstance b)
        {
            if (a.IsNull())
                throw new NullReferenceException(a.id);

            if (a.GetHybType().isPrimitive)
            {
                (a, b) = Promote(a, b);

                if (a.Is<Int32>()) return HybInstance.Int(a.As<Int32>() - b.As<Int32>());
                if (a.Is<Int64>()) return HybInstance.Int64(a.As<Int64>() - b.As<Int64>());
                if (a.Is<Single>()) return HybInstance.Float(a.As<Single>() - b.As<Single>());
                if (a.Is<Double>()) return HybInstance.Double(a.As<Double>() - b.As<Double>());
            }

            var subMethod = GetSubMethod(a);
            if (subMethod != null)
                return subMethod.target.Invoke(null, new HybInstance[] { a, b });

            throw new NotImplementedException();
        }
        public static HybInstance Mul(HybInstance a, HybInstance b)
        {
            if (a.IsNull())
                throw new NullReferenceException(a.id);

            if (a.GetHybType().isPrimitive)
            {
                (a, b) = Promote(a, b);

                if (a.Is<Int32>()) return HybInstance.Int(a.As<Int32>() * b.As<Int32>());
                if (a.Is<Int64>()) return HybInstance.Int64(a.As<Int64>() * b.As<Int64>());
                if (a.Is<Single>()) return HybInstance.Float(a.As<Single>() * b.As<Single>());
                if (a.Is<Double>()) return HybInstance.Double(a.As<Double>() * b.As<Double>());
            }

            var mulMethod = GetMulMethod(a);
            if (mulMethod != null)
                return mulMethod.target.Invoke(null, new HybInstance[] { a, b });

            throw new NotImplementedException();
        }
        public static HybInstance Div(HybInstance a, HybInstance b)
        {
            if (a.IsNull())
                throw new NullReferenceException(a.id);

            if (a.GetHybType().isPrimitive)
            {
                (a, b) = Promote(a, b);

                if (a.Is<Int32>()) return HybInstance.Int(a.As<Int32>() / b.As<Int32>());
                if (a.Is<Int64>()) return HybInstance.Int64(a.As<Int64>() / b.As<Int64>());
                if (a.Is<Single>()) return HybInstance.Float(a.As<Single>() / b.As<Single>());
                if (a.Is<Double>()) return HybInstance.Double(a.As<Double>() / b.As<Double>());
            }

            var DivMethod = GetDivMethod(a);
            if (DivMethod != null)
                return DivMethod.target.Invoke(null, new HybInstance[] { a, b });

            throw new NotImplementedException();
        }

        public static HybInstance G(HybInstance a, HybInstance b)
        {
            if (a.IsNull())
                throw new NullReferenceException(a.id);

            if (a.GetHybType().isPrimitive)
            {
                (a, b) = Promote(a, b);

                if (a.Is<Int32>()) return HybInstance.Bool(a.As<Int32>() > b.As<Int32>());
                if (a.Is<Int64>()) return HybInstance.Bool(a.As<Int64>() > b.As<Int64>());
                if (a.Is<Single>()) return HybInstance.Bool(a.As<Single>() > b.As<Single>());
                if (a.Is<Double>()) return HybInstance.Bool(a.As<Double>() > b.As<Double>());
            }

            var greaterMethod = GetGreaterMethod(a);
            if (greaterMethod != null)
                return greaterMethod.target.Invoke(null, new HybInstance[] { a, b });

            throw new NotImplementedException();
        }
        public static HybInstance GE(HybInstance a, HybInstance b)
        {
            if (a.IsNull())
                throw new NullReferenceException(a.id);

            if (a.GetHybType().isPrimitive)
            {
                (a, b) = Promote(a, b);

                if (a.Is<Int32>()) return HybInstance.Bool(a.As<Int32>() >= b.As<Int32>());
                if (a.Is<Int64>()) return HybInstance.Bool(a.As<Int64>() >= b.As<Int64>());
                if (a.Is<Single>()) return HybInstance.Bool(a.As<Single>() >= b.As<Single>());
                if (a.Is<Double>()) return HybInstance.Bool(a.As<Double>() >= b.As<Double>());
            }

            var greaterEqualMethod = GetGreaterEqualMethod(a);
            if (greaterEqualMethod != null)
                return greaterEqualMethod.target.Invoke(null, new HybInstance[] { a, b });

            throw new NotImplementedException();
        }
        public static HybInstance L(HybInstance a, HybInstance b)
        {
            if (a.IsNull())
                throw new NullReferenceException(a.id);

            if (a.GetHybType().isPrimitive)
            {
                (a, b) = Promote(a, b);

                if (a.Is<Int32>()) return HybInstance.Bool(a.As<Int32>() < b.As<Int32>());
                if (a.Is<Int64>()) return HybInstance.Bool(a.As<Int64>() < b.As<Int64>());
                if (a.Is<Single>()) return HybInstance.Bool(a.As<Single>() < b.As<Single>());
                if (a.Is<Double>()) return HybInstance.Bool(a.As<Double>() < b.As<Double>());
            }

            var lessMethod = GetLessMethod(a);
            if (lessMethod != null)
                return lessMethod.target.Invoke(null, new HybInstance[] { a, b });

            throw new NotImplementedException();
        }
        public static HybInstance LE(HybInstance a, HybInstance b)
        {
            if (a.IsNull())
                throw new NullReferenceException(a.id);

            if (a.GetHybType().isPrimitive)
            {
                (a, b) = Promote(a, b);

                if (a.Is<Int32>()) return HybInstance.Bool(a.As<Int32>() <= b.As<Int32>());
                if (a.Is<Int64>()) return HybInstance.Bool(a.As<Int64>() <= b.As<Int64>());
                if (a.Is<Single>()) return HybInstance.Bool(a.As<Single>() <= b.As<Single>());
                if (a.Is<Double>()) return HybInstance.Bool(a.As<Double>() <= b.As<Double>());
            }

            var lessEqualMethod = GetLessEqualMethod(a);
            if (lessEqualMethod != null)
                return lessEqualMethod.target.Invoke(null, new HybInstance[] { a, b });

            throw new NotImplementedException();
        }
        public static HybInstance Eq(HybInstance a, HybInstance b)
        {
            if (a.isCompiledType && b.isCompiledType)
            {
                var aType = a.GetHybType();
                if (aType.isValueType)
                {
                    if (aType.isPrimitive)
                        (a, b) = Promote(a, b);
                    return HybInstance.Bool(a.innerObject.Equals(b.innerObject));
                }
                return HybInstance.Bool(a.innerObject == b.innerObject);
            }

            throw new NotImplementedException();
        }
        public static HybInstance Neq(HybInstance a, HybInstance b)
        {
            if (a.isCompiledType && b.isCompiledType)
            {
                var aType = a.GetHybType();
                if (aType.isValueType)
                {
                    if (aType.isPrimitive)
                        (a, b) = Promote(a, b);
                    return HybInstance.Bool(!a.innerObject.Equals(b.innerObject));
                }
                return HybInstance.Bool(a.innerObject != b.innerObject);
            }

            throw new NotImplementedException();
        }

        public static HybInstance Or(HybInstance a, HybInstance b)
        {
            if (a.isCompiledType && b.isCompiledType)
            {
                if (a.Is<bool>() && b.Is<bool>())
                {
                    return a.As<bool>() || b.As<bool>() ? 
                        HybInstanceCache.True :
                        HybInstanceCache.False;
                }
            }

            throw new NotImplementedException();
        }
        public static HybInstance And(HybInstance a, HybInstance b)
        {
            if (a.isCompiledType && b.isCompiledType)
            {
                if (a.Is<bool>() && b.Is<bool>())
                {
                    return a.As<bool>() && b.As<bool>() ?
                        HybInstanceCache.True :
                        HybInstanceCache.False;
                }
            }

            throw new NotImplementedException();
        }
        public static HybInstance BitwiseAnd(HybInstance a, HybInstance b)
        {
            if (a.isCompiledType && b.isCompiledType)
            {
                if (a.Is<bool>() && b.Is<bool>())
                {
                    return a.As<bool>() & b.As<bool>() ?
                        HybInstanceCache.True :
                        HybInstanceCache.False;
                }
            }

            var bitwiseAndMethod = GetBitwiseAndMethod(a);
            if (bitwiseAndMethod != null)
                return bitwiseAndMethod.target.Invoke(null, new HybInstance[] { a, b });

            throw new NotImplementedException();
        }
        public static HybInstance BitwiseOr(HybInstance a, HybInstance b)
        {
            if (a.isCompiledType && b.isCompiledType)
            {
                if (a.Is<bool>() && b.Is<bool>())
                {
                    return a.As<bool>() | b.As<bool>() ?
                        HybInstanceCache.True :
                        HybInstanceCache.False;
                }
            }

            var bitwiseOrMethod = GetBitwiseOrMethod(a);
            if (bitwiseOrMethod != null)
                return bitwiseOrMethod.target.Invoke(null, new HybInstance[] { a, b });

            throw new NotImplementedException();
        }

        private static (HybInstance, HybInstance) Promote(HybInstance a, HybInstance b)
        {
            if (a.GetHybType() == b.GetHybType())
                return (a, b);

            if (a.Is<double>() || b.Is<double>())
                return (a.Cast<double>(), b.Cast<double>());
            if (a.Is<float>() || b.Is<float>())
                return (a.Cast<float>(), b.Cast<float>());
            if (a.Is<Int64>() || b.Is<Int64>())
                return (a.Cast<Int64>(), b.Cast<Int64>());
            if (a.Is<Int32>() || b.Is<Int32>())
                return (a.Cast<Int32>(), b.Cast<Int32>());

            return (a, b);
        }

        private static SSMethodInfo GetAddMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_Addition");
        private static SSMethodInfo GetSubMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_Subtraction");
        private static SSMethodInfo GetMulMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_Multiply");
        private static SSMethodInfo GetDivMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_Division");
        private static SSMethodInfo GetModMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_Modulus");
        private static SSMethodInfo GetBitwiseOrMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_BitwiseOr");
        private static SSMethodInfo GetBitwiseAndMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_BitwiseAnd");
        private static SSMethodInfo GetXorMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_ExclusiveOr");
        private static SSMethodInfo GetEqMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_Equality");
        private static SSMethodInfo GetGreaterMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_GreaterThan");
        private static SSMethodInfo GetLessMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_LessThan");
        private static SSMethodInfo GetGreaterEqualMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_GreaterThanOrEqual");
        private static SSMethodInfo GetLessEqualMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_LessThanOrEqual");

        private static SSMethodInfo GetOnesComplementMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_OnesComplement");
        private static SSMethodInfo GetLogicalNotMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_LogicalNot");
        private static SSMethodInfo GetUnaryNegationMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_UnaryNegation");
        private static SSMethodInfo GetUnaryPlusMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_UnaryPlus");

        private static SSMethodInfo GetIncMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_Increment");
        private static SSMethodInfo GetDecMethod(HybInstance left)
            => left.GetHybType().GetStaticMethodFirst("op_Decrement");
    }
}

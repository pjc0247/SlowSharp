using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal static class HybExt
    {
        public static object Unwrap(this HybInstance _this)
        {
            if (_this.isCompiledType)
                return _this.innerObject;
            return _this;
        }
        public static Type Unwrap(this HybType _this)
        {
            if (_this.isCompiledType)
                return _this.compiledType;
            return typeof(HybInstance);
        }
        public static Type[] Unwrap(this HybType[] _this)
        {
            var objs = new Type[_this.Length];

            for (int i = 0; i < _this.Length; i++)
                objs[i] = _this[i].compiledType;

            return objs;
        }
        public static object[] Unwrap(this HybInstance[] _this)
        {
            var objs = new object[_this.Length];

            for (int i = 0; i < _this.Length; i++)
                objs[i] = _this[i].Unwrap();

            return objs;
        }
        public static HybInstance[] Wrap(this object[] _this)
        {
            var objs = new HybInstance[_this.Length];

            for (int i = 0; i < _this.Length; i++)
                objs[i] = HybInstance.Object(_this[i]);

            return objs;
        }
        public static HybInstance Wrap(this object _this)
        {
            if (_this is HybInstance hyb) return hyb;
            return HybInstance.Object(_this);
        }

        public static bool IsAssignableFrom(this Type _this, HybType type)
        {
            return IsAssignableFromEx(_this, type, null);
        }
        public static bool IsAssignableFromEx(
            this Type _this, HybType type, Dictionary<string, Type> genericBound)
        {
            if (_this == typeof(object))
                return true;
            if (type.isCompiledType == false)
                return false;

            var cType = type.compiledType;
            if (_this.IsGenericType)
            {
                var gs = _this.GetGenericArguments();
                
                if (cType.IsGenericType &&
                    gs.Length == cType.GetGenericArguments().Length)
                {
                    try
                    {
                        var genericDefinition = _this.GetGenericTypeDefinition();
                        var cTypeGenericArgs = cType.GetGenericArguments();

                        if (genericBound != null)
                        {
                            var thisGenericArgs = _this.GetGenericArguments();
                            for (int i = 0; i < thisGenericArgs.Length; i++)
                                genericBound[thisGenericArgs[i].Name] = cTypeGenericArgs[i];
                        }

                        _this = genericDefinition
                            .MakeGenericType(cTypeGenericArgs);
                    }
                    catch (ArgumentException e)
                    {
                        return false;
                    }
                }
                else if (gs.Length == 1)
                {
                    var firstGenericArg = _this.GetGenericArguments().First();
                    _this = _this.GetGenericTypeDefinition();
                    Type[] genericArgs = new Type[] { cType };

                    if (_this == typeof(IEnumerable<>))
                    {
                        if (cType.IsArray)
                        {
                            var elemType = cType.GetElementType();

                            genericArgs = new Type[] { elemType };
                            if (genericBound != null)
                                genericBound[firstGenericArg.Name] = elemType;
                        }
                    }
                    else if (genericBound != null)
                        genericBound[firstGenericArg.Name] = cType;

                    try
                    {
                        _this = _this.MakeGenericType(genericArgs);
                    }
                    catch (ArgumentException e)
                    {
                        return false;
                    }
                }
            }

            return _this.IsAssignableFrom(type.compiledType);
        }
        public static bool IsSubclassOf(this Type _this, HybType type)
        {
            if (type.isCompiledType == false)
                return false;

            return _this.IsSubclassOf(type.compiledType);
        }
    }
}

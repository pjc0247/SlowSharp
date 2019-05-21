using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    /// <summary>
    /// SlowSharpFieldInfo
    /// </summary>
    public abstract class SSFieldInfo : SSMemberInfo
    {
        public HybType fieldType;
        public bool isConst;

        public abstract HybInstance GetValue(HybInstance _this);
        public abstract void SetValue(HybInstance _this, HybInstance value);
    }

    public class SSCompiledFieldInfo : SSFieldInfo
    {
        internal FieldInfo fieldInfo;

        internal SSCompiledFieldInfo(FieldInfo field)
        {
            this.Origin = SSMemberOrigin.InterpretScript;
            this.fieldType = HybTypeCache.GetHybType(field.FieldType);
            this.fieldInfo = field;

            this.IsStatic = field.IsStatic;
        }

        public override HybInstance GetValue(HybInstance _this)
            => HybInstance.Object(fieldInfo.GetValue(IsStatic ? null : _this.Unwrap()));
        public override void SetValue(HybInstance _this, HybInstance value)
            => fieldInfo.SetValue(IsStatic ? null : _this.Unwrap(), value);
    }
    public class SSInterpretFieldInfo : SSFieldInfo
    {
        internal VariableDeclaratorSyntax declartor;
        internal FieldDeclarationSyntax field;

        private int fieldPtr = -1;

        internal SSInterpretFieldInfo(Class klass)
        {
            this.Origin = SSMemberOrigin.InterpretScript;
            this.DeclaringClass = klass;
        }

        public override HybInstance GetValue(HybInstance _this)
        {
            if (IsStatic)
            {
                return DeclaringClass.Runner.Globals
                    .GetStaticField(DeclaringClass, Id);
            }

            // it's fast enough to do like this
            if (fieldPtr == -1)
                fieldPtr = _this.Fields.GetPtr(Id);
            return _this.Fields.GetByPtr(fieldPtr);
        }
        public override void SetValue(HybInstance _this, HybInstance value)
        {
            if (IsStatic)
            {
                DeclaringClass.Runner.Globals
                    .SetStaticField(DeclaringClass, Id, value);
                return;
            }

            // it's fast enough to do like this
            if (fieldPtr == -1)
                fieldPtr = _this.Fields.GetPtr(Id);
            _this.Fields.SetByPtr(fieldPtr, value);
        }
    }
}

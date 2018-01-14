using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Dora.DynamicProxy
{
    public static class ILGeneratorExtensions
    {
        private static readonly OpCode[] _loadArgsOpcodes =
        {
            OpCodes.Ldarg_1,
            OpCodes.Ldarg_2,
            OpCodes.Ldarg_3
        };        
        private static readonly OpCode[] _loadConstantInt32Opcodes =
        {
            OpCodes.Ldc_I4_0,
            OpCodes.Ldc_I4_1,
            OpCodes.Ldc_I4_2,
            OpCodes.Ldc_I4_3,
            OpCodes.Ldc_I4_4,
            OpCodes.Ldc_I4_5,
            OpCodes.Ldc_I4_6,
            OpCodes.Ldc_I4_7,
            OpCodes.Ldc_I4_8,
        };

        public static void EmitLoadArgument(this ILGenerator il, int index)
        {
            Guard.ArgumentNotNull(il, nameof(il));
            if (index < _loadArgsOpcodes.Length)
            {
                il.Emit(_loadArgsOpcodes[index]);
            }
            else
            {
                il.Emit(OpCodes.Ldarg, index + 1);
            }
        }
        public static void EmitLoadConstantInt32(this ILGenerator il, int number)
        {
            Guard.ArgumentNotNull(il, nameof(il));
            if (number <= 8)
            {
                il.Emit(_loadConstantInt32Opcodes[number]);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, number);
            }
        }  

        public static void EmitBox(this ILGenerator il, Type typeOnStack)
        {
            Guard.ArgumentNotNull(il, nameof(il));
            if (typeOnStack.IsValueType || typeOnStack.IsGenericParameter)
            {
                il.Emit(OpCodes.Box, typeOnStack);
            } 
            if (typeOnStack.IsByRef)
            {
                var nonByRefType = typeOnStack.GetNonByRefType();
                if (nonByRefType.IsValueType)
                {
                    il.Emit(OpCodes.Box, nonByRefType);
                }
            }
        }

        public static void EmitUnboxOrCast(this ILGenerator il, Type targetType)
        {
            Guard.ArgumentNotNull(il, nameof(il));
            if (targetType.IsValueType || targetType.IsGenericParameter)
            {
                il.Emit(OpCodes.Unbox_Any, targetType);
            }
            else if (targetType.IsByRef && (targetType.GetNonByRefType().IsValueType)) 
            {
                il.Emit(OpCodes.Unbox_Any, targetType.GetNonByRefType());
            }
            else
            {
                il.Emit(OpCodes.Castclass, targetType);
            }
        }

        public static void EmitLdInd(this ILGenerator il, Type targetType)
        {
            var nonByRefType = targetType.GetNonByRefType();

            if (nonByRefType == typeof(Int16))
            {
                il.Emit(OpCodes.Ldind_I2);
            }

            else if (nonByRefType == typeof(Int32))
            {
                il.Emit(OpCodes.Ldind_I4);
            }

            else if (nonByRefType == typeof(Int64))
            {
                il.Emit(OpCodes.Ldind_I8);
            }

            else if (nonByRefType == typeof(float))
            {
                il.Emit(OpCodes.Ldind_R4);
            }

            else if (nonByRefType == typeof(double))
            {
                il.Emit(OpCodes.Ldind_R8);
            }

            else if (nonByRefType == typeof(UInt16))
            {
                il.Emit(OpCodes.Ldind_U2);
            }

            else if (nonByRefType == typeof(UInt32))
            {
                il.Emit(OpCodes.Ldind_U4);
            }
            else
            {
                il.Emit(OpCodes.Ldind_Ref);
            }

        }

        public static void EmitStInd(this ILGenerator il, Type targetType)
        {
            var nonByRefType = targetType.GetNonByRefType();

            if (nonByRefType == typeof(Int16))
            {
                il.Emit(OpCodes.Stind_I2);
            }

            else if (nonByRefType == typeof(Int32))
            {
                il.Emit(OpCodes.Stind_I4);
            }
            else if (nonByRefType == typeof(Int64))
            {
                il.Emit(OpCodes.Stind_I8);
            }

            else if (nonByRefType == typeof(float))
            {
                il.Emit(OpCodes.Stind_R4);
            }

            else if (nonByRefType == typeof(double))
            {
                il.Emit(OpCodes.Stind_R8);
            }

            else
            {
                il.Emit(OpCodes.Stind_Ref);
            }  
        }
    }
}

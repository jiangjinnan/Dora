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
        }

        public static void EmitUnboxOrCast(this ILGenerator il, Type targetType)
        {
            Guard.ArgumentNotNull(il, nameof(il));
            if (targetType.IsValueType || targetType.IsGenericParameter)
            {
                il.Emit(OpCodes.Unbox_Any, targetType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, targetType);
            }
        }
    }
}

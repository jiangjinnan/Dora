using System;
using System.Reflection.Emit;

namespace Dora.Interception
{
    internal static class ILGeneratorExtensions
    {
        private static readonly OpCode[] _ldArgs = new OpCode[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 }; 
        public static ILGenerator EmitLdArgs(this ILGenerator il, int index)
        {
            if (index < _ldArgs.Length)
            {
                il.Emit(_ldArgs[index]);
            }
            else
            {
                il.Emit(OpCodes.Ldarg, index);
            }
            return il;
        }

        public static void EmitTryBox(this ILGenerator il, Type typeOnStack)
        {
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

            if (nonByRefType == typeof(short))
            {
                il.Emit(OpCodes.Ldind_I2);
            }

            else if (nonByRefType == typeof(int))
            {
                il.Emit(OpCodes.Ldind_I4);
            }

            else if (nonByRefType == typeof(long))
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

            if (nonByRefType == typeof(short))
            {
                il.Emit(OpCodes.Stind_I2);
            }

            else if (nonByRefType == typeof(int))
            {
                il.Emit(OpCodes.Stind_I4);
            }
            else if (nonByRefType == typeof(long))
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

        public static void EmitBox(this ILGenerator il, Type typeOnStack)
        {
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
    }
}

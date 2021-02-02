using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

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

    }
}

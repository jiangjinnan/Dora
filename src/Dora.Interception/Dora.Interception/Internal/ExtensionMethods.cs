using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dora.Interception
{
    internal static class ExtensionMethods
    {
        public static T[] Concat<T>(this T[] array1, T[] array2)
        {
            Guard.ArgumentNotNull(array1, nameof(array1));
            Guard.ArgumentNotNull(array2, nameof(array2));

            var lengthOfArray1 = array1.Length;
            var lengthOfArray2 = array2.Length;
            var array = new T[lengthOfArray1 + lengthOfArray2];

            if (array1.Length > 0)
            {
                for (int index = 0; index < lengthOfArray1; index++)
                {
                    array[index] = array1[index];
                }

                for (int index = 0; index < lengthOfArray2; index++)
                {
                    array[index + lengthOfArray1] = array2[index];
                }
            }

            return array;
        }
    }
}

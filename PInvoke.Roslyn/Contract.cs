using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Roslyn
{
    internal static class Contract
    {
        internal static Exception InvalidEnum<T>(T value)
        {
            return new Exception($"enum {typeof(T)} does not have the vaule {value}");
        }

        internal static void ThrowInvalidEnum<T>(T value)
        {
            throw InvalidEnum(value);
        }
    }
}

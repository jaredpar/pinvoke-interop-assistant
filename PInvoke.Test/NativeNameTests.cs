using PInvoke.Enums;
using PInvoke.NativeTypes;
using System;
using Xunit;

namespace PInvoke.Test
{
    public class NativeNameTests
    {
        [Fact]
        public void Equality()
        {
            EqualityUnit
                .Create(new NativeName("test", NativeNameKind.Struct))
                .WithEqualValues(new NativeName("test", NativeNameKind.Struct))
                .WithNotEqualValues(
                    new NativeName("test", NativeNameKind.Union),
                    new NativeName("other", NativeNameKind.Struct))
                .RunAll(
                    operatorEquals: (x, y) => x == y,
                    operatorNotEquals: (x, y) => x != y);
        }

        [Fact]
        public void SimpleConversions()
        {
            Action<NativeSymbol, NativeNameKind> test = (s, kind) =>
            {
                NativeName name;
                Assert.True(NativeNameUtil.TryGetName(s, out name));
                Assert.Equal(new NativeName(s.Name, kind), name);
            };

            test(new NativeStruct("test"), NativeNameKind.Struct);
            test(new NativeUnion("other"), NativeNameKind.Union);
            test(new NativeEnum("e"), NativeNameKind.Enum);
        }
    }
}

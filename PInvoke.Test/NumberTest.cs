using PInvoke.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PInvoke.Test
{
    public class NumberTest
    {
        [Fact]
        public void EqualsInteger()
        {
            EqualityUnit
                .Create(new Number(1))
                .WithEqualValues(new Number(1))
                .WithNotEqualValues(new Number(1L), new Number(42), Number.Empty)
                .RunAll(operatorEquals: (x, y) => x == y, operatorNotEquals: (x, y) => x != y);
        }

        [Fact]
        public void EqualsLong()
        {
            EqualityUnit
                .Create(new Number(1L))
                .WithEqualValues(new Number(1L))
                .WithNotEqualValues(new Number(1), new Number(42), Number.Empty)
                .RunAll(operatorEquals: (x, y) => x == y, operatorNotEquals: (x, y) => x != y);
        }
    }
}

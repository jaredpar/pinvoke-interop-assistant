using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Parser
{
    public enum NumberKind
    {
        Integer,
        Long,
        Single,
        Double
    }

    public struct Number : IEquatable<Number>
    {
        public static Number Empty => default(Number);

        public object Value { get; }
        public NumberKind Kind { get; }

        public int Integer => Get<int>(NumberKind.Integer);
        public long Long => Get<long>(NumberKind.Long);
        public float Single => Get<float>(NumberKind.Single);
        public double Double => Get<double>(NumberKind.Double);
        public bool IsEmpty => Value == null;

        public Number(int i)
        {
            Value = i;
            Kind = NumberKind.Integer;
        }

        public Number(long l)
        {
            Value = l;
            Kind = NumberKind.Long;
        }

        public Number(float f)
        {
            Value = f;
            Kind = NumberKind.Single;
        }

        public Number(double d)
        {
            Value = d;
            Kind = NumberKind.Double;
        }

        public int ConvertToInteger()
        {
            switch (Kind)
            {
                case NumberKind.Integer: return Integer;
                case NumberKind.Long: return (int)Long;
                case NumberKind.Single: return (int)Single;
                case NumberKind.Double: return (int)Double;
                default: throw Contract.CreateInvalidEnumValueException(Kind);
            }
        }

        public long ConvertToLong()
        {
            switch (Kind)
            {
                case NumberKind.Integer: return Integer;
                case NumberKind.Long: return Long;
                case NumberKind.Single: return (long)Single;
                case NumberKind.Double: return (long)Double;
                default: throw Contract.CreateInvalidEnumValueException(Kind);
            }
        }

        public double ConvertToDouble()
        {
            switch (Kind)
            {
                case NumberKind.Integer: return Integer;
                case NumberKind.Long: return Long;
                case NumberKind.Single: return Single;
                case NumberKind.Double: return Double;
                default: throw Contract.CreateInvalidEnumValueException(Kind);
            }
        }

        private T Get<T>(NumberKind kind)
        {
            Contract.Requires(kind == Kind);
            return (T)Value;
        }

        public static bool TryCreate(object value, out Number number)
        {
            var type = value.GetType();
            if (type == typeof(int))
            {
                number = new Number((int)value);
                return true;
            }

            if (type == typeof(long))
            {
                number = new Number((long)value);
                return true;
            }

            if (type == typeof(float))
            {
                number = new Number((float)value);
                return true;
            }

            if (type == typeof(double))
            {
                number = new Number((double)value);
                return true;
            }

            number = Empty;
            return false;
        }

        public static bool operator ==(Number left, Number right)
        {
            if (left.IsEmpty)
            {
                return right.IsEmpty;
            }

            return left.Kind == right.Kind && left.Value.Equals(right.Value);
        }

        public static bool operator !=(Number left, Number right) => !(left == right);
        public bool Equals(Number other) => this == other;
        public override bool Equals(object obj) => obj is Number && Equals((Number)obj);
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
        public override string ToString() => $"Value {Value} Kind {Kind}";
    }
}

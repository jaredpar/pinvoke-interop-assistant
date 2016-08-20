// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Xunit;

namespace PInvoke.Test
{
    public sealed class EqualityUnit<T>
    {
        /// <summary>
        /// Main class which does a lot of the boiler plate work for testing that the equality pattern
        /// is properly implemented in objects
        /// </summary>
        internal sealed class Impl
        {
            private readonly EqualityUnit<T> _equalityUnit;
            private readonly Func<T, T, bool> _operatorEqual;
            private readonly Func<T, T, bool> _operatorNotEqual;

            public Impl(
                EqualityUnit<T> equalityUnit,
                Func<T, T, bool> compEquality = null,
                Func<T, T, bool> compInequality = null)
            {
                _equalityUnit = equalityUnit;
                _operatorEqual = compEquality;
                _operatorNotEqual = compInequality;
            }

            public void RunAll(bool checkIEquatable = true)
            {
                if (_operatorEqual != null)
                {
                    OperatorEqualsStandard();
                    OperatorEqualsReferenceTypes();
                }

                if (_operatorNotEqual != null)
                {
                    OperatorNotEqualsStandard();
                    OperatorNotEqualsReferenceTypes();
                }

                if (checkIEquatable)
                {
                    ImplementsIEquatable();
                }

                ObjectEqualsSymmetric();
                ObjectEqualsNull();
                ObjectEqualsDifferentType();
                GetHashCodeBehavior();

                if (checkIEquatable)
                {
                    EquatableEqualsStandard();
                    EquatableEqualsReferenceType();
                }
            }

            /// <summary>
            /// Test behaior of operator==, operator!= for provided equals and not equals values.
            /// </summary>
            private void OperatorEqualsStandard()
            {
                foreach (var value in _equalityUnit.EqualValues)
                {
                    Assert.True(_operatorEqual(_equalityUnit.Value, value));
                    Assert.True(_operatorEqual(value, _equalityUnit.Value));
                }

                foreach (var value in _equalityUnit.NotEqualValues)
                {
                    Assert.False(_operatorEqual(_equalityUnit.Value, value));
                    Assert.False(_operatorEqual(value, _equalityUnit.Value));
                }
            }

            /// <summary>
            /// Test behavior of operator==, operator!= specific to reference types.  For all non-null
            /// values the operator should compare falsely to null. 
            /// </summary>
            private void OperatorEqualsReferenceTypes()
            {
                if (typeof(T).GetTypeInfo().IsValueType)
                {
                    return;
                }

                foreach (var value in _equalityUnit.AllValues)
                {
                    if ((object)value == null)
                    {
                        continue;
                    }

                    Assert.False(_operatorEqual(default(T), value));
                    Assert.False(_operatorEqual(value, default(T)));
                }

                Assert.True(_operatorEqual(default(T), default(T)));
            }

            private void OperatorNotEqualsStandard()
            {
                foreach (var value in _equalityUnit.EqualValues)
                {
                    Assert.False(_operatorNotEqual(_equalityUnit.Value, value));
                    Assert.False(_operatorNotEqual(value, _equalityUnit.Value));
                }

                foreach (var value in _equalityUnit.NotEqualValues)
                {
                    Assert.True(_operatorNotEqual(_equalityUnit.Value, value));
                    Assert.True(_operatorNotEqual(value, _equalityUnit.Value));
                }
            }

            private void OperatorNotEqualsReferenceTypes()
            {
                if (typeof(T).GetTypeInfo().IsValueType)
                {
                    return;
                }

                foreach (var value in _equalityUnit.AllValues)
                {
                    if ((object)value == null)
                    {
                        continue;
                    }

                    Assert.True(_operatorNotEqual(default(T), value));
                    Assert.True(_operatorNotEqual(value, default(T)));
                }

                Assert.False(_operatorNotEqual(default(T), default(T)));
            }

            private void ImplementsIEquatable()
            {
                var type = typeof(T);
                var targetType = typeof(IEquatable<T>);
                Assert.True(type.GetTypeInfo().ImplementedInterfaces.Contains(targetType));
            }

            private void ObjectEqualsSymmetric()
            {
                var unitValue = _equalityUnit.Value;
                foreach (var value in _equalityUnit.EqualValues)
                {
                    Assert.True(value.Equals(unitValue));
                    Assert.True(unitValue.Equals(value));
                }
            }

            /// <summary>
            /// Comparison with Null should be false for all types
            /// </summary>
            private void ObjectEqualsNull()
            {
                var allValues = _equalityUnit.AllValues;
                foreach (var value in allValues)
                {
                    if ((object)value == null)
                    {
                        continue;
                    }
                    Assert.False(value.Equals(null));
                }
            }

            /// <summary>
            /// Passing a value of a different type should just return false
            /// </summary>
            private void ObjectEqualsDifferentType()
            {
                var allValues = _equalityUnit.AllValues;
                foreach (var value in allValues)
                {
                    Assert.False(value.Equals((object)42));
                }
            }

            private void GetHashCodeBehavior()
            {
                foreach (var value in _equalityUnit.EqualValues)
                {
                    Assert.Equal(value.GetHashCode(), _equalityUnit.Value.GetHashCode());
                }
            }

            private void EquatableEqualsStandard()
            {
                var equatableUnit = (IEquatable<T>)_equalityUnit.Value;
                foreach (var value in _equalityUnit.EqualValues)
                {
                    Assert.True(equatableUnit.Equals(value));
                    var equatableValue = (IEquatable<T>)value;
                    Assert.True(equatableValue.Equals(_equalityUnit.Value));
                }

                foreach (var value in _equalityUnit.NotEqualValues)
                {
                    Assert.False(equatableUnit.Equals(value));
                    var equatableValue = (IEquatable<T>)value;
                    Assert.False(equatableValue.Equals(_equalityUnit.Value));
                }
            }

            /// <summary>
            /// If T is a reference type, null should return false in all cases
            /// </summary>
            private void EquatableEqualsReferenceType()
            {
                if (typeof(T).GetTypeInfo().IsValueType)
                {
                    return;
                }

                foreach (var cur in _equalityUnit.AllValues)
                {
                    if (cur == null)
                    {
                        continue;
                    }

                    var value = (IEquatable<T>)cur;
                    Assert.False(value.Equals((T)(object)null));
                }
            }
        }

        private static readonly ReadOnlyCollection<T> s_emptyCollection = new ReadOnlyCollection<T>(new T[] { });

        public readonly T Value;
        public readonly ReadOnlyCollection<T> EqualValues;
        public readonly ReadOnlyCollection<T> NotEqualValues;
        public IEnumerable<T> AllValues
        {
            get { return Enumerable.Repeat(Value, 1).Concat(EqualValues).Concat(NotEqualValues); }
        }

        public EqualityUnit(T value)
        {
            Value = value;
            EqualValues = s_emptyCollection;
            NotEqualValues = s_emptyCollection;
        }

        public EqualityUnit(
            T value,
            ReadOnlyCollection<T> equalValues,
            ReadOnlyCollection<T> notEqualValues)
        {
            Value = value;
            EqualValues = equalValues;
            NotEqualValues = notEqualValues;
        }

        public EqualityUnit<T> WithEqualValues(params T[] equalValues)
        {
            return new EqualityUnit<T>(
                Value,
                EqualValues.Concat(equalValues).ToList().AsReadOnly(),
                NotEqualValues);
        }

        public EqualityUnit<T> WithNotEqualValues(params T[] notEqualValues)
        {
            return new EqualityUnit<T>(
                Value,
                EqualValues,
                NotEqualValues.Concat(notEqualValues).ToList().AsReadOnly());
        }

        public void RunAll(
            bool checkIEquatable = true,
            Func<T, T, bool> operatorEquals = null,
            Func<T, T, bool> operatorNotEquals = null)
        {
            var impl = new Impl(this, operatorEquals, operatorNotEquals);
            impl.RunAll(checkIEquatable);
        }
    }

    public static class EqualityUnit
    {
        public static EqualityUnit<T> Create<T>(T value) => new EqualityUnit<T>(value);
    }
}
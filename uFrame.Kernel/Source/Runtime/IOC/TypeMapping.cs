using System;

namespace uFrame.IOC
{
    public struct TypeMapping : IEquatable<TypeMapping>
    {
        public Type From { get; }
        public Type To { get; }

        public TypeMapping(Type from, Type to)
        {
            From = from;
            To = to;
        }

        #region Equality Checks

        public bool Equals(TypeMapping other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TypeMapping && Equals((TypeMapping) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((From != null ? From.GetHashCode() : 0) * 397) ^ (To != null ? To.GetHashCode() : 0);
            }
        }

        #endregion

        #region Equality Operators

        public static bool operator ==(TypeMapping a, TypeMapping b)
        {
            return a.From == b.From && a.To == b.To;
        }

        public static bool operator !=(TypeMapping a, TypeMapping b)
        {
            return !(a == b);
        }

        #endregion
    }
}
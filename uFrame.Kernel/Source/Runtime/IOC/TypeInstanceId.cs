using System;

namespace uFrame.IOC
{
    public struct TypeInstanceId : IEquatable<TypeInstanceId>
    {
        public Type Type { get; }
        public string Name { get; }

        public TypeInstanceId(Type type) : this(type, string.Empty)
        {
        }

        public TypeInstanceId(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Type?.Name}/{Name}";
        }

        #region Equality Checks

        public bool Equals(TypeInstanceId other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TypeInstanceId && Equals((TypeInstanceId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        #endregion

        #region Equality Operators

        public static bool operator ==(TypeInstanceId a, TypeInstanceId b)
        {
            return a.Type == b.Type && string.Equals(a.Name, b.Name);
        }

        public static bool operator !=(TypeInstanceId a, TypeInstanceId b)
        {
            return !(a == b);
        }

        #endregion
    }
}
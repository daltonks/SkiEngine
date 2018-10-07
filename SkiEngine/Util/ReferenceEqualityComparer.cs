using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SkiEngine.Util
{
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer, IEqualityComparer<T>
    {
        public static ReferenceEqualityComparer<T> Default { get; } = new ReferenceEqualityComparer<T>();

        public bool Equals(T x, T y) => ReferenceEquals(x, y);
#pragma warning disable 108,114
        public bool Equals(object x, object y) => ReferenceEquals(x, y);
#pragma warning restore 108,114

        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}

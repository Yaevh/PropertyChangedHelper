using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyChangedHelper
{
    public static class TypeExtensions
    {
        public static bool DerivesFrom(this Type current, Type other)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return other.IsAssignableFrom(current);
        }
    }
}

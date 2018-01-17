//this file copied from stackoverflow: http://stackoverflow.com/questions/26280788/dictionary-enum-key-performance

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NewtonVR {
  internal struct EnumEqualityComparer<TEnum> : IEqualityComparer<TEnum> where TEnum : struct {
    private static class BoxAvoidance {
      private static readonly Func<TEnum, int> _wrapper;

      static BoxAvoidance() {
        var p = Expression.Parameter(typeof(TEnum), null);
        var c = Expression.ConvertChecked(p, typeof(int));

        _wrapper = Expression.Lambda<Func<TEnum, int>>(c, p).Compile();
      }

      public static int ToInt(TEnum enu) {
        return _wrapper(enu);
      }
    }

    public bool Equals(TEnum firstEnum, TEnum secondEnum) {
      return BoxAvoidance.ToInt(firstEnum) == BoxAvoidance.ToInt(secondEnum);
    }

    public int GetHashCode(TEnum firstEnum) {
      return BoxAvoidance.ToInt(firstEnum);
    }
  }
}

using System.Collections.Generic;

namespace NewtonVR {
  public struct NVRButtonsComparer : IEqualityComparer<NVRButtons> {
    public bool Equals(NVRButtons x, NVRButtons y) {
      return x == y;
    }

    public int GetHashCode(NVRButtons obj) {
      return (int) obj;
    }
  }
}

using UnityEngine;

namespace NewtonVR.Example {
  public class NVRExampleSwitchResult : MonoBehaviour {
    private Light SpotLight;
    public NVRSwitch Switch;

    private void Awake() {
      SpotLight = GetComponent<Light>();
    }

    private void Update() {
      SpotLight.enabled = Switch.CurrentState;
    }
  }
}

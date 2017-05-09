using UnityEngine;

namespace NewtonVR.Example {
  public class NVRExampleRGBResult : MonoBehaviour {
    public Renderer Result;
    public NVRSlider SliderBlue;
    public NVRSlider SliderGreen;
    public NVRSlider SliderRed;

    private void Update() {
      Result.material.color = new Color(SliderRed.CurrentValue, SliderGreen.CurrentValue,
        SliderBlue.CurrentValue);
    }
  }
}

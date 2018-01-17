using UnityEngine;

namespace NewtonVR.Example {
  public class NVRExampleColorSlider : MonoBehaviour {
    public Color From;

    public Renderer Result;

    public NVRSlider Slider;
    public Color To;

    private void Update() {
      Result.material.color = Color.Lerp(From, To, Slider.CurrentValue);
    }
  }
}

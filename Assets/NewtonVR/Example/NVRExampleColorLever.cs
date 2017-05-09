using UnityEngine;

namespace NewtonVR.Example {
  public class NVRExampleColorLever
    : MonoBehaviour {
    public Color From;

    public NVRLever Lever;

    public Renderer Result;
    public Color To;

    private void Update() {
      Result.material.color = Color.Lerp(From, To, Lever.CurrentValue);
    }
  }
}

using UnityEngine;

namespace NewtonVR.Example {
  public class NVRExampleEvent : MonoBehaviour {
    public void Duplicate() {
      GameObject duplicate = Instantiate(gameObject);
      duplicate.transform.Translate(0, Random.value, 0, Space.World);
      duplicate.GetComponent<NVRInteractableItem>().ResetInteractable();
    }
  }
}

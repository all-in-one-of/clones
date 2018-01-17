using NewtonVR;
using UnityEngine;

public class DestroyInteractables : MonoBehaviour {
  public void OnCollisionEnter(Collision collision) {
    if (collision.gameObject.GetComponentInParent<NVRInteractable>() != null) {
      Destroy(collision.gameObject);
    }
  }
}

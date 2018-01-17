using System.Linq;
using MoreLinq;
using UnityEngine;
using Valve.VR;

public class Hand : MonoBehaviour {
  private SpringJoint existing_joint;
  private SteamVR_TrackedObject hand;

  private void Awake() {
    hand = GetComponent<SteamVR_TrackedObject>();
  }

  // Update is called once per frame
  private void Update() {
    SteamVR_Controller.Device device = SteamVR_Controller.Input((int) hand.index);
    bool trigger_pressed = device.GetPress(EVRButtonId.k_EButton_SteamVR_Trigger);
    bool trigger_down = device.GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger);
    bool trigger_up = device.GetPressUp(EVRButtonId.k_EButton_SteamVR_Trigger);

    float scale = 1.2f;
    transform.GetChild(0).localScale = trigger_pressed ? new Vector3(scale, scale, scale) : new Vector3(1, 1, 1);

    if (trigger_down) {
      Debug.Log("Trigger down");
      Collider[] overlapping_arr = Physics.OverlapSphere(transform.position, 1f);
      var overlapping = overlapping_arr.Where(c => c.gameObject.GetComponent<Rigidbody>() != null);

      if (overlapping.Any()) {
        Collider closest_collider = overlapping.MinBy(c => Vector3.Distance(c.transform.position, transform.position));

        existing_joint = closest_collider.gameObject.AddComponent<SpringJoint>();
        existing_joint.connectedBody = GetComponent<Rigidbody>();

        Debug.Log("Grabbed an object!");
      }
    }

    if (trigger_up) {
      if (existing_joint != null) {
        Destroy(existing_joint);
        existing_joint = null;
        Debug.Log("Grabbed an object!");
      }
    }

    if (existing_joint != null) {
      transform.GetChild(0).localScale = new Vector3(1.6f, 1.6f, 1.6f);
    }
  }
}

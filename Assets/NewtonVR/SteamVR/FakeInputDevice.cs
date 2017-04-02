using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Optional;

namespace NewtonVR {
  public class FakeInputDevice : NVRInputDevice {

    public override void TriggerHapticPulse(ushort durationMicroSec = 500, NVRButtons button = NVRButtons.Touchpad) {
    }

    public override float GetAxis1D(NVRButtons button) {
      return 0;
    }

    public override Vector2 GetAxis2D(NVRButtons button) {
      return Vector2.zero;
    }

    public override bool GetPressDown(NVRButtons button) {
      if (button == NVRButtons.Grip) {
		    return GetComponent<PlaybackActions>().playback_cursor.hold_down || Input.GetKeyDown(KeyCode.Space);
      }
      return false;
    }

    public override bool GetPressUp(NVRButtons button) {
      if (button == NVRButtons.Grip) {
				return GetComponent<PlaybackActions>().playback_cursor.hold_up || Input.GetKeyUp(KeyCode.Space);
      }
      return false;
    }

    public override bool GetPress(NVRButtons button) {
      if (button == NVRButtons.Grip) {
				return GetComponent<PlaybackActions>().playback_cursor.hold_pressed || Input.GetKey(KeyCode.Space);
      }
      return false;
    }

    public override bool GetTouchDown(NVRButtons button) {
      return false;
    }

    public override bool GetTouchUp(NVRButtons button) {
      return false;
    }

    public override bool GetTouch(NVRButtons button) {
      return false;
    }

    public override bool GetNearTouchDown(NVRButtons button) {
      return false;
    }

    public override bool GetNearTouchUp(NVRButtons button) {
      return false;
    }

    public override bool GetNearTouch(NVRButtons button) {
      return false;
    }

    public override bool IsCurrentlyTracked {
      get { return true; }
    }

    public override Option<GameObject> SetupDefaultRenderModel() {
      return Option.None<GameObject>();
    }

    public override bool ReadyToInitialize() {
      return true;
    }

    public override string GetDeviceName() {
      return "Fake Hand";
    }

    public override Collider[] SetupDefaultPhysicalColliders(Transform ModelParent) {
      Collider[] colliders = null;

      Transform dk2TrackhatColliders = ModelParent.transform.FindChild("ViveColliders");
      if (dk2TrackhatColliders == null) {
        dk2TrackhatColliders =
          GameObject.Instantiate(Resources.Load<GameObject>("ViveControllers/ViveColliders"))
                    .transform;
        dk2TrackhatColliders.parent = ModelParent.transform;
        dk2TrackhatColliders.localPosition = Vector3.zero;
        dk2TrackhatColliders.localRotation = Quaternion.identity;
        dk2TrackhatColliders.localScale = Vector3.one;
      }

      colliders = dk2TrackhatColliders.GetComponentsInChildren<Collider>();

      return colliders;
    }

    public override Collider[] SetupDefaultColliders() {
      Collider[] colliders = null;
      colliders = new Collider[] {};
      return colliders;
    }
  }
}
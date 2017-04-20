using Optional;
using UnityEngine;
using UnityEngine.Assertions;

namespace NewtonVR {
  public class FakeInputDevice : NVRInputDevice {
    private long current_frame;
    private Option<RecordActions.Snapshot> current_frame_snapshot;

    // TODO: this should be the previous snapshot in the playback buffer, not just the last one we've inspected.
    private Option<RecordActions.Snapshot> last_frame_snapshot;

    public override bool IsCurrentlyTracked {
      get { return true; }
    }

    private void Update() {
      Assert.IsTrue(Time.frameCount >= current_frame);
      if (Time.frameCount != current_frame) {
        last_frame_snapshot = current_frame_snapshot;
        current_frame_snapshot = GetComponent<PlaybackActions>().playback_cursor.ToOption();
        current_frame = Time.frameCount;
      }
    }

    public override void TriggerHapticPulse(ushort durationMicroSec = 500,
                                            NVRButtons button = NVRButtons.Touchpad) {
    }

    public override float GetAxis1D(NVRButtons button) {
      return current_frame_snapshot.UnwrapOrDefault(snapshot => snapshot.axis[button].x, 0f);
    }

    public override Vector2 GetAxis2D(NVRButtons button) {
      return current_frame_snapshot.UnwrapOrDefault(snapshot => snapshot.axis[button], Vector2.zero);
    }

    public override bool GetPressDown(NVRButtons button) {
      bool last_press = last_frame_snapshot.UnwrapOrDefault(snapshot => snapshot.press_down[button],
        false);
      bool current_press =
        current_frame_snapshot.UnwrapOrDefault(snapshot => snapshot.press_down[button], false);

      return current_press && !last_press; // Only fire if this input wasn't true last frame
    }

    public override bool GetPressUp(NVRButtons button) {
      bool last_press = last_frame_snapshot.UnwrapOrDefault(snapshot => snapshot.press_up[button],
        false);
      bool current_press =
        current_frame_snapshot.UnwrapOrDefault(snapshot => snapshot.press_up[button], false);

      return current_press && !last_press; // Only fire if this input wasn't true last frame
    }

    public override bool GetPress(NVRButtons button) {
      return current_frame_snapshot.UnwrapOrDefault(snapshot => snapshot.pressed[button], false);
    }

    public override bool GetTouchDown(NVRButtons button) {
      bool last_touch = last_frame_snapshot.UnwrapOrDefault(snapshot => snapshot.touch_down[button],
        false);
      bool current_touch =
        current_frame_snapshot.UnwrapOrDefault(snapshot => snapshot.touch_down[button], false);

      return current_touch && !last_touch; // Only fire if this input wasn't true last frame
    }

    public override bool GetTouchUp(NVRButtons button) {
      bool last_touch = last_frame_snapshot.UnwrapOrDefault(snapshot => snapshot.touch_up[button],
        false);
      bool current_touch =
        current_frame_snapshot.UnwrapOrDefault(snapshot => snapshot.touch_up[button], false);

      return current_touch && !last_touch; // Only fire if this input wasn't true last frame
    }

    public override bool GetTouch(NVRButtons button) {
      return current_frame_snapshot.UnwrapOrDefault(snapshot => snapshot.touched[button], false);
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
      Transform dk2TrackhatColliders = ModelParent.transform.FindChild("ViveColliders");

      if (dk2TrackhatColliders == null) {
        dk2TrackhatColliders =
          Instantiate(Resources.Load<GameObject>("ViveControllers/ViveColliders"))
            .transform;
        dk2TrackhatColliders.parent = ModelParent.transform;
        dk2TrackhatColliders.localPosition = Vector3.zero;
        dk2TrackhatColliders.localRotation = Quaternion.identity;
        dk2TrackhatColliders.localScale = Vector3.one;
      }

      return dk2TrackhatColliders.GetComponentsInChildren<Collider>();
    }

    public override Collider[] SetupDefaultColliders() {
      return new Collider[] {};
    }
  }
}

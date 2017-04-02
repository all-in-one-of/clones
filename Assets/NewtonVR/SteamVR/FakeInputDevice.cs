using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Optional;

namespace NewtonVR {
  public class FakeInputDevice : NVRInputDevice {
    private Dictionary<NVRButtons, EVRButtonId> ButtonMapping =
      new Dictionary<NVRButtons, EVRButtonId>(new NVRButtonsComparer());

    public override void Initialize(NVRHand hand) {
      SetupButtonMapping();

      base.Initialize(hand);

      if (Hand.CurrentHandState != HandState.Uninitialized) {
        Hand.Initialize();
      }
    }

    protected virtual void SetupButtonMapping() {
      ButtonMapping.Add(NVRButtons.A, EVRButtonId.k_EButton_A);
      ButtonMapping.Add(NVRButtons.ApplicationMenu, EVRButtonId.k_EButton_ApplicationMenu);
      ButtonMapping.Add(NVRButtons.Axis0, EVRButtonId.k_EButton_Axis0);
      ButtonMapping.Add(NVRButtons.Axis1, EVRButtonId.k_EButton_Axis1);
      ButtonMapping.Add(NVRButtons.Axis2, EVRButtonId.k_EButton_Axis2);
      ButtonMapping.Add(NVRButtons.Axis3, EVRButtonId.k_EButton_Axis3);
      ButtonMapping.Add(NVRButtons.Axis4, EVRButtonId.k_EButton_Axis4);
      ButtonMapping.Add(NVRButtons.Back, EVRButtonId.k_EButton_Dashboard_Back);
      ButtonMapping.Add(NVRButtons.DPad_Down, EVRButtonId.k_EButton_DPad_Down);
      ButtonMapping.Add(NVRButtons.DPad_Left, EVRButtonId.k_EButton_DPad_Left);
      ButtonMapping.Add(NVRButtons.DPad_Right, EVRButtonId.k_EButton_DPad_Right);
      ButtonMapping.Add(NVRButtons.DPad_Up, EVRButtonId.k_EButton_DPad_Up);
      ButtonMapping.Add(NVRButtons.Grip, EVRButtonId.k_EButton_Grip);
      ButtonMapping.Add(NVRButtons.System, EVRButtonId.k_EButton_System);
      ButtonMapping.Add(NVRButtons.Touchpad, EVRButtonId.k_EButton_SteamVR_Touchpad);
      ButtonMapping.Add(NVRButtons.Trigger, EVRButtonId.k_EButton_SteamVR_Trigger);


      ButtonMapping.Add(NVRButtons.B, EVRButtonId.k_EButton_A);
      ButtonMapping.Add(NVRButtons.X, EVRButtonId.k_EButton_A);
      ButtonMapping.Add(NVRButtons.Y, EVRButtonId.k_EButton_A);
    }

    private EVRButtonId GetButton(NVRButtons button) {
      if (ButtonMapping.ContainsKey(button) == false) {
        return EVRButtonId.k_EButton_System;
        //Debug.LogError("No SteamVR button configured for: " + button.ToString());
      }
      return ButtonMapping[button];
    }

    public override void TriggerHapticPulse(ushort durationMicroSec = 500, NVRButtons button = NVRButtons.Touchpad) {
      Debug.Log("No haptic ability in fake controller.");
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
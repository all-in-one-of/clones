using System;
using System.Collections.Generic;
using System.Linq;
using NewtonVR;
using UnityEngine;

public class RecordActions : MonoBehaviour {
  public struct Snapshot {
    public Vector3 position;
    public Quaternion rotation;
    public double timestamp;
    public Dictionary<NVRButtons, bool> pressed;
    public Dictionary<NVRButtons, bool> press_down;
    public Dictionary<NVRButtons, bool> press_up;
    public Dictionary<NVRButtons, bool> touched;
    public Dictionary<NVRButtons, bool> touch_down;
    public Dictionary<NVRButtons, bool> touch_up;
    public Dictionary<NVRButtons, Vector2> axis;
  }

  public LineRenderer line_renderer;

  public List<Snapshot> snapshots = new List<Snapshot>();
  private bool record_toggle;

  /// <summary>
  ///   Extracts all button states for a single state (ispressed, isdown, etc) into a dictionary.
  /// </summary>
  private Dictionary<NVRButtons, T> RecordButtonsForInputState<T>(Dictionary<NVRButtons, NVRButtonInputs> true_inputs,
                                                                  Dictionary<NVRButtons, T> outputs,
                                                                  Func
                                                                  <Dictionary<NVRButtons, NVRButtonInputs>, NVRButtons,
                                                                    T> extraction_func) {
    foreach (NVRButtons button in NVRButtonsHelper.Array) {
      outputs[button] = extraction_func(true_inputs, button);
    }
    return outputs;
  }

  public void FixedUpdate() {
    /*if (record_toggle) {
      NVRHand hand = GetComponent<NVRHand>();
      SaveNewRecordingSnapshot(hand);
    }*/
    NVRHand hand = GetComponent<NVRHand>();
    //Debug.Log("Frame " + Time.frameCount + " FixedUpdate " + hand.transform.position.x);
  }

  public void Update() {
    NVRHand hand = GetComponent<NVRHand>();
    //Debug.Log("Frame " + Time.frameCount + " UPDATE      " + hand.transform.position.x);
    if (hand.CurrentHandState == HandState.Uninitialized) {
      return;
    }

    if (Input.GetKeyUp(KeyCode.Space) || hand.Inputs[NVRButtons.ApplicationMenu].PressDown) {
      if (!record_toggle) {
        record_toggle = true;
      } else {
        record_toggle = false;
        CreateFake(hand, new List<Snapshot>(snapshots));
        snapshots.Clear();
      }
    }

    if (record_toggle) {
      SaveNewRecordingSnapshot(hand);
    }

    // Display path
    if (line_renderer != null) {
      line_renderer.positionCount = snapshots.Count;
      line_renderer.SetPositions(snapshots.Select(s => s.position).ToArray());
    }
  }

  /// <summary>
  ///   Saves a new snapshot of the inputs values for the current recording.
  /// </summary>
  private void SaveNewRecordingSnapshot(NVRHand hand) {
    Snapshot snap = new Snapshot {
      position = transform.position,
      rotation = transform.rotation,
      timestamp = Time.realtimeSinceStartup,
      pressed =
        RecordButtonsForInputState(hand.Inputs, new Dictionary<NVRButtons, bool>(),
          (inputs_dict, button) => inputs_dict[button].IsPressed),
      press_down =
        RecordButtonsForInputState(hand.Inputs, new Dictionary<NVRButtons, bool>(),
          (inputs_dict, button) => inputs_dict[button].PressDown),
      press_up =
        RecordButtonsForInputState(hand.Inputs, new Dictionary<NVRButtons, bool>(),
          (inputs_dict, button) => inputs_dict[button].PressUp),
      touched =
        RecordButtonsForInputState(hand.Inputs, new Dictionary<NVRButtons, bool>(),
          (inputs_dict, button) => inputs_dict[button].IsTouched),
      touch_down =
        RecordButtonsForInputState(hand.Inputs, new Dictionary<NVRButtons, bool>(),
          (inputs_dict, button) => inputs_dict[button].TouchDown),
      touch_up =
        RecordButtonsForInputState(hand.Inputs, new Dictionary<NVRButtons, bool>(),
          (inputs_dict, button) => inputs_dict[button].TouchUp),
      axis =
        RecordButtonsForInputState(hand.Inputs, new Dictionary<NVRButtons, Vector2>(),
          (inputs_dict, button) => inputs_dict[button].Axis)
    };
    snapshots.Add(snap);
  }

  private GameObject CreateFake(NVRHand real_hand, List<Snapshot> recording) {
    // Disable the hand before cloning anything:
    real_hand.gameObject.SetActive(false);

    GameObject fake_hand_obj = Instantiate(real_hand.gameObject, real_hand.transform.parent, true);

    fake_hand_obj.name = fake_hand_obj.name = real_hand.name + " [Puppet]";

    // Recreate hand component:
    DestroyImmediate(fake_hand_obj.GetComponent<NVRHand>());
    var fake_hand = fake_hand_obj.AddComponent<NVRHand>();

    Type[] remove_types = {
      typeof(SteamVR_RenderModel), typeof(SteamVR_TrackedObject), typeof(NVRSteamVRInputDevice),
      typeof(NVRPhysicalController), typeof(RecordActions)
    };

    // Remove bad components from children. (anything with a global reference, essentially)
    Component[] components = fake_hand_obj.GetComponentsInChildren<Component>(true);
    foreach (var component in components) {
      if (remove_types.Contains(component.GetType())) {
        DestroyImmediate(component);
      }
    }

    var device = fake_hand_obj.AddComponent<FakeInputDevice>();
    var playback = fake_hand_obj.AddComponent<PlaybackActions>();
    playback.Recording = recording;

    real_hand.gameObject.SetActive(true);
    fake_hand_obj.SetActive(true);

    fake_hand.PreInitialize(real_hand.Player);
    fake_hand.SetupInputDevice(device);
    real_hand.Player.Hands.Add(fake_hand);

    return fake_hand_obj;
  }

  //    fake_hand_obj.name = fake_hand_obj.name = real_hand.name + " [Puppet]";
  //
  //    GameObject fake_hand_obj = new GameObject();
  //    // Disable the hand before cloning anything:
  //  private GameObject CreateFake(NVRHand real_hand, List<Snapshot> recording) {
  /// </summary>
  /// Doesn't yet have a recording set to it.
  /// Creates a puppet hand as a clone of the current state of the hand.

  /// <summary>
  //    fake_hand_obj.transform.SetParent(real_hand.Player.transform);
  //
  //    var fake_hand = fake_hand_obj.AddComponent<NVRHand>();
  //    var device = fake_hand_obj.AddComponent<FakeInputDevice>();
  //    var playback = fake_hand_obj.AddComponent<PlaybackActions>();
  //    playback.recording = recording;
  //
  //    fake_hand_obj.AddComponent<HandControls>();
  //
  //    // Duplicate children (render models, associated objects).
  //    foreach (Transform child in real_hand.transform) {
  //      child.gameObject.SetActive(false);
  //      var child_clone = Instantiate(child.gameObject, fake_hand_obj.transform, false);
  //      child_clone.name = child_clone.name.Replace("(Clone)", "");
  //      child.gameObject.SetActive(true);
  //    }
  //
  //    // Remove bad components from children. (anything with a global reference, essentially)
  //    Component[] components = fake_hand_obj.GetComponentsInChildren<Component>(true);
  //    foreach (var component in components) {
  //      if (component.GetType() == typeof(SteamVR_RenderModel)) {
  //        DestroyImmediate(component);
  //      }
  //    }
  //
  //    // Enable children
  //    foreach (Transform child in fake_hand_obj.transform) {
  //      child.gameObject.SetActive(true);
  //    }
  //
  //    fake_hand.PreInitialize(real_hand.Player);
  //    fake_hand.SetupInputDevice(device);
  //    real_hand.Player.Hands.Add(fake_hand);
  //
  //    return fake_hand_obj;
  //  }
}

using System;
using System.Collections;
using System.Collections.Generic;
using NewtonVR;
using UnityEngine;
using System.Linq;
using MoreLinq;

public class RecordActions : MonoBehaviour {
  public LineRenderer line_renderer;

  public struct Snapshot {
    public Vector3 position;
    public Quaternion rotation;
    public bool hold_down;
    public bool hold_up;
    public bool hold_pressed;
    public double timestamp;
  }

  public List<Snapshot> snapshots = new List<Snapshot>();


  public void Update() {
    NVRHand hand = GetComponent<NVRHand>();
    if (hand.CurrentHandState == HandState.Uninitialized) { return; }

    if (Input.GetKeyUp(KeyCode.Space) || hand.UseButtonUp) {
      CreateFake(hand, new List<Snapshot>(snapshots));
      snapshots.Clear();
    }

    if (hand.UseButtonPressed) {
      Snapshot snap = new Snapshot() {
        position = transform.position,
        rotation = transform.rotation,
        hold_down = hand.HoldButtonDown,
        hold_up = hand.HoldButtonUp,
        hold_pressed = hand.HoldButtonPressed,
        timestamp = Time.realtimeSinceStartup
      };
      snapshots.Add(snap);
    }

    // Display path
    if (line_renderer != null) {
      line_renderer.numPositions = snapshots.Count;
      line_renderer.SetPositions(snapshots.Select(s => s.position).ToArray());
    }
  }

  private void CreateFake(NVRHand real_hand, List<Snapshot> recording) {
    // Disable the hand before cloning anything:
    GameObject fake_hand_obj = new GameObject();

    fake_hand_obj.name = fake_hand_obj.name = real_hand.name + " [Puppet]";
    fake_hand_obj.transform.SetParent(real_hand.Player.transform);

    var fake_hand = fake_hand_obj.AddComponent<NVRHand>();
    var device = fake_hand_obj.AddComponent<FakeInputDevice>();

    var playback = fake_hand_obj.AddComponent<PlaybackActions>();
    playback.recording = recording;

    // Duplicate children (render models, associated objects).
    foreach (Transform child in real_hand.transform) {
      child.gameObject.SetActive(false);
      var child_clone = Instantiate(child.gameObject, fake_hand_obj.transform, false);
      child_clone.name = child_clone.name.Replace("(Clone)", "");
      child.gameObject.SetActive(true);
    }

    // Remove bad components from children. (anything with a global reference, essentially)
    Component[] components = fake_hand_obj.GetComponentsInChildren<Component>(true);
    foreach (var component in components) {
      if (component.GetType() == typeof(SteamVR_RenderModel)) {
        DestroyImmediate(component);
      }
    }

    // Enable children
    foreach (Transform child in fake_hand_obj.transform) {
      child.gameObject.SetActive(true);
    }

    fake_hand.PreInitialize(real_hand.Player);
    fake_hand.SetupInputDevice(device);
    real_hand.Player.Hands.Add(fake_hand);
  }
}

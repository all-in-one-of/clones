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
    if (hand.UseButtonDown) {
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

    if (Input.GetKeyDown(KeyCode.Space)) {
      CreateFake(hand);
    }
  }

  private void CreateFake(NVRHand real_hand) {
    GameObject fake_hand_obj = Instantiate(real_hand.gameObject);
    fake_hand_obj.transform.SetParent(real_hand.Player.transform);
    var fake_hand = fake_hand_obj.GetComponent<NVRHand>();

    // Remove tracking and recording

    switch (real_hand.Player.CurrentIntegrationType) {
      case NVRSDKIntegrations.SteamVR:
        Destroy(fake_hand_obj.GetComponent<SteamVR_TrackedObject>());
        Destroy(fake_hand_obj.GetComponent<NVRSteamVRInputDevice>());
        break;
      case NVRSDKIntegrations.None:
      case NVRSDKIntegrations.FallbackNonVR:
      case NVRSDKIntegrations.Oculus:
      default:
        Debug.LogError("TODO: No support for Occulus/NonSteam VR yet.");
        throw new ArgumentOutOfRangeException();
    }
    Destroy(fake_hand_obj.GetComponent<RecordActions>());
    fake_hand.PhysicalController = null;

    var device = fake_hand_obj.AddComponent<FakeInputDevice>();
    var playback = fake_hand_obj.AddComponent<PlaybackActions>();
    playback.recording = real_hand.GetComponent<RecordActions>();

    fake_hand.PreInitialize(real_hand.Player);
    fake_hand.SetupInputDevice(device);
    real_hand.Player.Hands.Add(fake_hand);
  }
}

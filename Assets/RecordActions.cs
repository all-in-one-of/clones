using System;
using System.Collections;
using System.Collections.Generic;
using NewtonVR;
using UnityEngine;
using System.Linq;
using MoreLinq;

public class RecordActions : MonoBehaviour
{
  public LineRenderer line_renderer;

  public struct Snapshot
  {
    public Vector3 position;
    public Quaternion rotation;
    public bool hold_down;
    public bool hold_up;
    public bool hold_pressed;
    public double timestamp;
  }

  public List<Snapshot> snapshots = new List<Snapshot> ();


  void Update ()
  {
    NVRHand hand = GetComponent<NVRHand> ();
    if (hand.UseButtonDown) {
      snapshots.Clear ();
    }
    if (hand.UseButtonPressed) {
      Snapshot snap = new Snapshot () {
        position = transform.position,
        rotation = transform.rotation,
        hold_down = hand.HoldButtonDown,
        hold_up = hand.HoldButtonUp,
        hold_pressed = hand.HoldButtonPressed,
        timestamp = Time.realtimeSinceStartup
      };
      snapshots.Add (snap);
    }

    // Display path
    if (line_renderer != null) {
      line_renderer.numPositions = snapshots.Count;
      line_renderer.SetPositions (snapshots.Select (s => s.position).ToArray ());
    }
  }

}

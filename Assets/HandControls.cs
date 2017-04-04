using System.Collections;
using System.Collections.Generic;
using MoreLinq;
using NewtonVR;
using UnityEngine;

public class HandControls : MonoBehaviour {

  // Update is called once per frame
  public void Update() {
    NVRHand hand = GetComponent<NVRHand>();

    if (hand.Inputs[NVRButtons.ApplicationMenu].PressDown) {
      var all_playing_clones = FindObjectsOfType<PlaybackActions>();
      var closest_clone = all_playing_clones.MinBy(x => Vector3.Distance(x.transform.position, transform.position));
      Destroy(closest_clone.gameObject);
    }
  }
}

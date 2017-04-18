using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using MoreLinq;
using NewtonVR;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
using Valve.VR;

public class HandControls : MonoBehaviour {

  private abstract class Tool {
    public Color color;
    /// Called every update while this tool is the active tool.
    /// Note: Called on the frame in which we swap to the tool, and not on the frame in which we swap away.
    public abstract void Update(NVRHand hand);

    /// <summary>
    /// Called whenever we swap away from this tool.
    /// </summary>
    public virtual void ChangeAwayFrom(NVRHand hand) { }

    /// <summary>
    /// Called whenever we swap to this tool.
    /// </summary>
    public virtual void ChangeTo(NVRHand hand) { }
  }

  private class Transport : Tool {
    public Transport() {
      color = Color.white;
    }
    public override void Update(NVRHand hand) {
      // Nothing extra beyond gripping
    }
  }

  private class Duplicate : Tool {
    public Duplicate() {
      color = Color.blue;
    }
    public override void Update(NVRHand hand) {
      if (hand.UseButtonDown && hand.IsInteracting) {
        GameObject duplicate = Instantiate(hand.CurrentlyInteracting.gameObject);
        duplicate.GetComponent<NVRInteractable>().ResetInteractable();

        // Find bounds of all colliders and subcolliders on object.
        var colliders = hand.CurrentlyInteracting.gameObject.GetComponentsInChildren<Collider>();
        var bounds = colliders.Select(c => c.bounds).Aggregate((b1, b2) => {
          var b = new Bounds {
            center = b1.center,
            extents = b1.extents
          };

          b.Encapsulate(b2);
          return b;
        });

        // Place new duplicate object above held object.
        duplicate.transform.position = hand.CurrentlyInteracting.gameObject.transform.position -
                                       new Vector3(0, bounds.size.y, 0);
      }
    }
  }

  private class MoveRecordings : Tool {
    private readonly Color hover_color = new Color(.5f,0,0,1f);
    private readonly Color nonhover_color = new Color(0,0,0,.2f);


    private Vector3? last_position = null;
    private PlaybackActions current_hover = null;
    public MoveRecordings() {
      color = Color.green;
    }

    public override void Update(NVRHand hand) {
      // Highlight the recording we're closest to the start point of.
      var all_playing_clones = FindObjectsOfType<PlaybackActions>();
      if (all_playing_clones.Length != 0) {
        var closest_clone = all_playing_clones.MinBy(x => Vector3.Distance(x.transform.position, hand.transform.position));
        if (closest_clone != current_hover) {
          if (current_hover != null) {
            current_hover.line_renderer.material.color = nonhover_color;
          }
          closest_clone.line_renderer.material.color = hover_color;
          current_hover = closest_clone;
        }
      }

      // Move the recording when we grip the trigger
      if (hand.UseButtonPressed) {
        if (last_position.HasValue) {
          if (current_hover != null) {
            var shift = hand.CurrentPosition - last_position.Value;
            Debug.Log(shift);

            // shift all recorded positions, by the delta
            var shifted_recording = current_hover.Recording.Select(snapshot => {
              snapshot.position = snapshot.position + shift;
              return snapshot;
            }).ToList();
            current_hover.Recording = shifted_recording;
          }
        }

        last_position = hand.CurrentPosition;
      }

      if (hand.UseButtonUp) {
        last_position = null;
      }
    }

    public override void ChangeAwayFrom(NVRHand hand) {
      if (current_hover != null) {
        current_hover.line_renderer.material.color = nonhover_color;
      }
      current_hover = null;
    }
  }

  private Tool[] tools = {
    new Transport(),
    new Duplicate(),
    new MoveRecordings(), 
  };


  public int tool = 0;

  // Update is called once per frame
  public void Update() {
    NVRHand hand = GetComponent<NVRHand>();
    if (hand.Inputs[NVRButtons.Touchpad].PressDown) {
      Debug.Log(GetDPadPress(hand));
    }

    // Delete nearest recording, DPAD_DOWN
    if (GetDPadPress(hand) == NVRButtons.DPad_Down) {
      var all_playing_clones = FindObjectsOfType<PlaybackActions>();
      if (all_playing_clones.Length != 0) {
      var closest_clone = all_playing_clones.MinBy(x => Vector3.Distance(x.transform.position, transform.position));
      Destroy(closest_clone.gameObject);
      }
    }

    // Change tool, DPAD_LEFT
    if (GetDPadPress(hand) == NVRButtons.DPad_Left || Input.GetKeyDown(KeyCode.LeftArrow)) {
      tools[tool].ChangeAwayFrom(hand);
      tool = (tool + 1) % tools.Length;
      tools[tool].ChangeTo(hand);
      Tool current_tool = tools[tool];
      GetComponentsInChildren<Renderer>().ForEach(x => { x.material.color = current_tool.color; });
    }

    // Reset scene, DPAD_UP
    if (GetDPadPress(hand) == NVRButtons.DPad_Up) {
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    tools[tool].Update(hand);
  }



  /*
   *  You might expect that pressing one of the edges of the SteamVR controller touchpad could
   *  be detected with a call to device.GetPress( EVRButtonId.k_EButton_DPad_* ), but currently this always returns false.
   *  Not sure whether this is SteamVR's design intent, not yet implemented, or a bug.
   *  The expected behaviour can be achieved by detecting overall Touchpad press, with Touch-Axis comparison to an edge threshold.
   */
  public static NVRButtons? GetDPadPress(NVRHand hand) {
    if (hand.Inputs[NVRButtons.Touchpad].PressDown) {
      var touchpad_axis = hand.Inputs[NVRButtons.Touchpad].Axis;
      var angle = Mathf.Rad2Deg * Mathf.Atan2(touchpad_axis.y, touchpad_axis.x);

      if (NumUtils.DistanceInModulo(angle, 0, 360) < 45) {
        return NVRButtons.DPad_Right;
      } else if (NumUtils.DistanceInModulo(angle, 90, 360) < 45) {
        return NVRButtons.DPad_Up;
      } else if (NumUtils.DistanceInModulo(angle, 180, 360) < 45) {
        return NVRButtons.DPad_Left;
      } else if (NumUtils.DistanceInModulo(angle, 270, 360) < 45) {
        return NVRButtons.DPad_Down;
      } else {
        Debug.LogError("Error: DPAD Press Math is wrong. Angle did not register with any specified direction.");
      }
    }

    return null;
  }
}

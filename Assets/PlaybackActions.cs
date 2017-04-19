using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Utilities;
using Debug = System.Diagnostics.Debug;

public class PlaybackActions : MonoBehaviour {
  public static double sequence_period = 5.0;

  /// <summary>
  /// The method to use to get the global sequence timer.
  /// </summary>
  public static Func<double> sequence_time = () => Time.realtimeSinceStartup;

  public List<RecordActions.Snapshot> Recording
  {
    get { return recording; }
    set
    {
      recording = value;
      UpdateLineRenderer();
    }
  }

  public RecordActions.Snapshot? playback_cursor;
  public LineRenderer line_renderer;
  private List<RecordActions.Snapshot> recording;

  /// <summary>
  /// Runs before the first Update() call.
  /// </summary>
  public void Start() {
    // Create a line renderer for this object if we don't have one.
    if (line_renderer == null) {
      GameObject line_obj = (GameObject) Instantiate(Resources.Load("PlaybackLine"));
      line_obj.name = "PlaybackLine (" + gameObject.name + ")";
      line_renderer = line_obj.GetComponent<LineRenderer>();

      // Line starts invisible
      line_renderer.material.color = new Color(0, 0, 0, 0);
    }

    UpdateLineRenderer();
  }

  // Set the path for the line
  private void UpdateLineRenderer() {
    if (line_renderer != null) {
      line_renderer.numPositions = recording.Count;
      line_renderer.SetPositions(recording.Select(s => s.position).ToArray());
    }
  }

  // Update is called once per frame
  public void Update() {
    if (recording.Count > 0) {
      // start time is the end of the last loop

      //double total_recording_length =
      //  NumUtils.NextHighestMultiple(recording.Last().timestamp - recording.First().timestamp, sequence_period);
      double total_recording_length = sequence_period; // Testing fixed period

      double recording_start_time_original = recording.First().timestamp;
      double playback_start_time = recording.First().timestamp;

      double playback_timer = (sequence_time() - playback_start_time) % total_recording_length;
      double playback_time_original = playback_timer + recording_start_time_original;

      playback_cursor = recording[0];
      foreach (RecordActions.Snapshot snap in recording) {
        if (snap.timestamp > playback_time_original) {
          break;
        }
        playback_cursor = snap;
      }

      // update to match recording
      transform.position = playback_cursor.Value.position;
      transform.rotation = playback_cursor.Value.rotation;
    }
  }

  public void OnDestroy() {
    if (line_renderer != null) {
      Destroy(line_renderer.gameObject);
    }
  }
}

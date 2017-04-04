using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

public class PlaybackActions : MonoBehaviour {
  public List<RecordActions.Snapshot> recording;
	public RecordActions.Snapshot playback_cursor;

	// Update is called once per frame
	public void Update () {
	  if (recording.Count > 0) {
      // start time is the end of the last loop
	    double sequence_period = 5.0;
	    double sequence_time = Time.realtimeSinceStartup;
	    double total_recording_length =
	      NumUtils.NextHighestMultiple(recording.Last().timestamp - recording.First().timestamp, sequence_period);

	    double recording_start_time_original = recording.First().timestamp;
	    double playback_start_time = recording.First().timestamp;

	    double playback_timer = (sequence_time - playback_start_time) % total_recording_length;
	    double playback_time_original = playback_timer + recording_start_time_original;

	    playback_cursor = recording[0];
      foreach (RecordActions.Snapshot snap in recording) {
        if (snap.timestamp > playback_time_original) {
          break;
        }
        playback_cursor = snap;
      }

			// update to match recording
	    transform.position = playback_cursor.position;
	    transform.rotation = playback_cursor.rotation;
	  }
 	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaybackActions : MonoBehaviour {
  public List<RecordActions.Snapshot> recording;
	public RecordActions.Snapshot playback_cursor;

	// Update is called once per frame
	public void Update () {
	  if (recording.Count > 0) {
      // start time is the end of the last loop
	    double recording_start_time = recording.First().timestamp;
	    double playback_start_time = recording.Last().timestamp;
	    double total_recording_length = recording.Last().timestamp - recording.First().timestamp;
	    double playback_time = (Time.realtimeSinceStartup - playback_start_time) % total_recording_length;
	    double playback_time_original = playback_time + recording_start_time;

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

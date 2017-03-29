using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaybackActions : MonoBehaviour {
  public RecordActions recording;
	public RecordActions.Snapshot playback_cursor;

	
	// Update is called once per frame
	void Update () {
	  var snapshots = recording.snapshots;
	  if (snapshots.Count > 0) {
      // start time is the end of the last loop
	    double recording_start_time = snapshots.First().timestamp;
	    double playback_start_time = snapshots.Last().timestamp;
	    double total_recording_length = snapshots.Last().timestamp - snapshots.First().timestamp;
	    double playback_time = (Time.realtimeSinceStartup - playback_start_time) % total_recording_length;
	    double playback_time_original = playback_time + recording_start_time;

	    playback_cursor = snapshots[0];
      foreach (RecordActions.Snapshot snap in snapshots) {
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

using UnityEngine;

public class Metronome : MonoBehaviour {
  // Use this for initialization
  private void Start() {
  }

  // Update is called once per frame
  private void Update() {
    // 0..1 
    double progress = (PlaybackActions.sequence_time() / PlaybackActions.sequence_period) % 1;

    // Set the size
    Vector3 scale = transform.localScale;
    scale.x = (float) (1 - progress) * .05f;
    transform.localScale = scale;

    // Set the color
    GetComponent<Renderer>().material.color = progress > .8 ? Color.red : Color.white;
  }
}

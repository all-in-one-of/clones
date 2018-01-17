using UnityEngine;

public class BoxGoal : MonoBehaviour {
  private double last_tick;
  private int total;

  // Update is called once per frame
  private void Update() {
    if (Time.time - last_tick > 1.0 && total < 4) {
      total = Mathf.Max(0, total - 1);
      last_tick = Time.time;
    }

    var walls = gameObject.transform.parent.GetComponentsInChildren<BoxCollider>();
    for (int i = 0; i < 4; i++) {
      Color color = i < total ? Color.blue : Color.white;
      if (total >= 4) {
        color = Color.green;
      }

      walls[i].gameObject.GetComponent<MeshRenderer>().material.color = color;
    }
  }

  public void OnTriggerEnter(Collider other) {
    Debug.Log(other.gameObject.tag);
    if (other.gameObject.tag.Equals("ScoreBlock")) {
      total += 1;
    }
  }
}

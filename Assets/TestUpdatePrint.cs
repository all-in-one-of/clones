using UnityEngine;

public class TestUpdatePrint : MonoBehaviour {
  private void Update() {
    Debug.Log("UpdatePrint: " + Time.frameCount);
    Debug.Log("Update is being called!");
  }
}

using UnityEngine;

public class TestEnableDisable : MonoBehaviour {
  public bool DisableEnable = false;

  private void Update() {
    Debug.Log("EnableDisable: " + Time.frameCount);
    if (DisableEnable) {
      gameObject.SetActive(false);
      gameObject.SetActive(true);
    }
  }
}

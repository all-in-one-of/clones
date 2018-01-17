﻿using UnityEngine;

namespace NewtonVR.Example {
  public class NVRExampleSpawner : MonoBehaviour {
    public NVRButton Button;

    public GameObject ToCopy;

    private void Update() {
      if (Button.ButtonDown) {
        GameObject newGo = Instantiate(ToCopy);
        newGo.transform.position = transform.position + new Vector3(1, 1, 0);
        newGo.transform.localScale = ToCopy.transform.lossyScale;
      }
    }
  }
}

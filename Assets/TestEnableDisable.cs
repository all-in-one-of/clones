using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnableDisable : MonoBehaviour {
  public bool DisableEnable = false;
	void Update () {
	  Debug.Log("EnableDisable: " + Time.frameCount);
	  if (DisableEnable) {
	    gameObject.SetActive(false);
	    gameObject.SetActive(true);
	  }
	}
}

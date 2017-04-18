using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUpdatePrint : MonoBehaviour {
	void Update () {
	  Debug.Log("UpdatePrint: " + Time.frameCount);
	  Debug.Log("Update is being called!");
	}
}

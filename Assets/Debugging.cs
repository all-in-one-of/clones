using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Debugging : MonoBehaviour {

    private void OnEnable()
    {
        Debug.Log("uv: " + GetComponent<MeshFilter>().sharedMesh.uv.Length);
        Debug.Log("uv2: " + GetComponent<MeshFilter>().sharedMesh.uv2.Length);
        Debug.Log("uv3: " + GetComponent<MeshFilter>().sharedMesh.uv3.Length);
        Debug.Log("uv4: " + GetComponent<MeshFilter>().sharedMesh.uv4.Length);

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

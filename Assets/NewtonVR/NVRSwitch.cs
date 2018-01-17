﻿using UnityEngine;

namespace NewtonVR {
  public class NVRSwitch : MonoBehaviour {
    public bool CurrentState = true;
    public bool LastState = true;

    public Transform OffButton;
    public Renderer OffButtonRenderer;

    public Transform OnButton;
    public Renderer OnButtonRenderer;
    private bool FixedState = true;
    private readonly float ForceMagic = 100f;

    private Rigidbody Rigidbody;

    private void Awake() {
      Rigidbody = GetComponent<Rigidbody>();
      SetRotation(CurrentState);
    }

    private void FixedUpdate() {
      float angle = transform.localEulerAngles.z;
      if (angle > 180) angle -= 360;

      if (angle > -7.5f) {
        if (angle < -0.2f) {
          Rigidbody.AddForceAtPosition(-transform.right * ForceMagic, OnButton.position);
        } else if ((angle > -0.2f && angle < -0.1f) || angle > 0.1f) {
          SetRotation(true);
        }
      } else if (angle < -7.5f) {
        if (angle > -14.8f) {
          Rigidbody.AddForceAtPosition(-transform.right * ForceMagic, OffButton.position);
        } else if ((angle < -14.8f && angle > -14.9f) || angle < -15.1) {
          SetRotation(false);
        }
      }
    }

    private void Update() {
      LastState = CurrentState;
      CurrentState = FixedState;
    }

    private void SetRotation(bool forState) {
      FixedState = forState;
      if (FixedState) {
        transform.localEulerAngles = Vector3.zero;
        OnButtonRenderer.material.color = Color.yellow;
        OffButtonRenderer.material.color = Color.white;
      } else {
        transform.localEulerAngles = new Vector3(0, 0, -15);
        OnButtonRenderer.material.color = Color.white;
        OffButtonRenderer.material.color = Color.red;
      }

      Rigidbody.angularVelocity = Vector3.zero;
      Rigidbody.velocity = Vector3.zero;
    }
  }
}

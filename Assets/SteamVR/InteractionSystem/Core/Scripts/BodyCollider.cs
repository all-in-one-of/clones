﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Collider dangling from the player's head
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem {
  //-------------------------------------------------------------------------
  [RequireComponent(typeof(CapsuleCollider))]
  public class BodyCollider : MonoBehaviour {
    public Transform head;

    private CapsuleCollider capsuleCollider;

    //-------------------------------------------------
    private void Awake() {
      capsuleCollider = GetComponent<CapsuleCollider>();
    }

    //-------------------------------------------------
    private void FixedUpdate() {
      float distanceFromFloor = Vector3.Dot(head.localPosition, Vector3.up);
      capsuleCollider.height = Mathf.Max(capsuleCollider.radius, distanceFromFloor);
      transform.localPosition = head.localPosition - 0.5f * distanceFromFloor * Vector3.up;
    }
  }
}

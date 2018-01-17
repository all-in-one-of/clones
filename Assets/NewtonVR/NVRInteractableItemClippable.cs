using UnityEngine;

namespace NewtonVR {
  /// <summary>
  ///   This interactable item script clips through other colliders. If you don't want your item to respect other object's
  ///   positions
  ///   and have it go through walls/floors/etc then you can use this.
  /// </summary>
  public class NVRInteractableItemClippable : NVRInteractable {
    [Tooltip(
      "If you have a specific point you'd like the object held at, create a transform there and set it to this variable"
    )] public Transform InteractionPoint;

    protected Transform PickupTransform;

    protected override void Awake() {
      base.Awake();
      Rigidbody.maxAngularVelocity = 100f;
    }

    protected void FixedUpdate() {
      if (IsAttached) {
        Vector3 TargetPosition;
        Quaternion TargetRotation;

        if (InteractionPoint != null) {
          TargetRotation = AttachedHand.transform.rotation * Quaternion.Inverse(InteractionPoint.localRotation);
          TargetPosition = transform.position + (AttachedHand.transform.position - InteractionPoint.position);
        } else {
          TargetRotation = PickupTransform.rotation;
          TargetPosition = PickupTransform.position;
        }

        Rigidbody.MovePosition(TargetPosition);
        Rigidbody.MoveRotation(TargetRotation);
      }
    }

    public override void BeginInteraction(NVRHand hand) {
      base.BeginInteraction(hand);

      Rigidbody.isKinematic = true;

      PickupTransform = new GameObject(string.Format("[{0}] NVRPickupTransform", gameObject.name)).transform;
      PickupTransform.parent = hand.transform;
      PickupTransform.position = transform.position;
      PickupTransform.rotation = transform.rotation;
    }

    public override void EndInteraction() {
      base.EndInteraction();

      Rigidbody.isKinematic = false;

      if (PickupTransform != null) Destroy(PickupTransform.gameObject);
    }
  }
}

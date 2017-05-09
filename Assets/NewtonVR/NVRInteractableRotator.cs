using UnityEngine;

namespace NewtonVR {
  public class NVRInteractableRotator : NVRInteractable {
    public float CurrentAngle;

    protected Transform InitialAttachPoint;

    protected virtual float DeltaMagic {
      get { return 1f; }
    }

    protected override void Awake() {
      base.Awake();
      Rigidbody.maxAngularVelocity = 100f;
    }

    protected virtual void FixedUpdate() {
      if (IsAttached) {
        Vector3 PositionDelta = (AttachedHand.transform.position - InitialAttachPoint.position) *
                                DeltaMagic;

        Rigidbody.AddForceAtPosition(PositionDelta, InitialAttachPoint.position,
          ForceMode.VelocityChange);
      }

      CurrentAngle = Quaternion.Angle(Quaternion.identity, transform.rotation);
    }

    public override void BeginInteraction(NVRHand hand) {
      base.BeginInteraction(hand);

      InitialAttachPoint =
        new GameObject(string.Format("[{0}] InitialAttachPoint", gameObject.name)).transform;
      InitialAttachPoint.position = hand.transform.position;
      InitialAttachPoint.rotation = hand.transform.rotation;
      InitialAttachPoint.localScale = Vector3.one * 0.25f;
      InitialAttachPoint.parent = transform;
    }

    public override void EndInteraction() {
      base.EndInteraction();

      if (InitialAttachPoint != null) {
        Destroy(InitialAttachPoint.gameObject);
      }
    }
  }
}

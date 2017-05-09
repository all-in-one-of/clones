using UnityEngine;

namespace NewtonVR {
  public abstract class NVRInteractable : MonoBehaviour {
    public NVRHand AttachedHand;

    public bool CanAttach = true;
    protected Vector3 ClosestHeldPoint;

    protected Collider[] Colliders;

    public bool DisableKinematicOnAttach = true;
    public float DropDistance = 1;

    public bool EnableGravityOnDetach = true;
    public bool EnableKinematicOnDetach = false;
    public Rigidbody Rigidbody;

    public virtual bool IsAttached {
      get { return AttachedHand != null; }
    }

    protected virtual void Awake() {
      if (Rigidbody == null) {
        Rigidbody = GetComponent<Rigidbody>();
      }

      if (Rigidbody == null) {
        Debug.LogError("There is no rigidbody attached to this interactable.");
      }
    }

    protected virtual void Start() {
      UpdateColliders();
    }

    public virtual void ResetInteractable() {
      Awake();
      Start();
      AttachedHand = null;
    }

    public virtual void UpdateColliders() {
      Colliders = GetComponentsInChildren<Collider>();
      NVRInteractables.Register(this, Colliders);
    }

    protected virtual bool CheckForDrop() {
      float shortestDistance = float.MaxValue;

      for (int index = 0; index < Colliders.Length; index++) {
        //todo: this does not do what I think it does.
        Vector3 closest = Colliders[index].ClosestPointOnBounds(AttachedHand.transform.position);
        float distance = Vector3.Distance(AttachedHand.transform.position, closest);

        if (distance < shortestDistance) {
          shortestDistance = distance;
          ClosestHeldPoint = closest;
        }
      }

      if (DropDistance != -1 && AttachedHand.CurrentInteractionStyle != InterationStyle.ByScript &&
          shortestDistance > DropDistance) {
        DroppedBecauseOfDistance();
        return true;
      }

      return false;
    }

    //Remove items that go too high or too low.
    protected virtual void Update() {
      if (transform.position.y > 10000 || transform.position.y < -10000) {
        if (AttachedHand != null) {
          AttachedHand.EndInteraction(this);
        }

        Destroy(gameObject);
      }
    }

    public virtual void BeginInteraction(NVRHand hand) {
      AttachedHand = hand;

      if (DisableKinematicOnAttach) {
        Rigidbody.isKinematic = false;
      }
    }

    public virtual void InteractingUpdate(NVRHand hand) {
      if (hand.UseButtonUp) {
        UseButtonUp();
      }

      if (hand.UseButtonDown) {
        UseButtonDown();
      }
    }

    public virtual void HoveringUpdate(NVRHand hand, float forTime) {
    }

    public void ForceDetach() {
      if (AttachedHand != null) {
        AttachedHand.EndInteraction(this);
      }

      if (AttachedHand != null) {
        EndInteraction();
      }
    }

    public virtual void EndInteraction() {
      AttachedHand = null;
      ClosestHeldPoint = Vector3.zero;

      if (EnableKinematicOnDetach) {
        Rigidbody.isKinematic = true;
      }

      if (EnableGravityOnDetach) {
        Rigidbody.useGravity = true;
      }
    }

    protected virtual void DroppedBecauseOfDistance() {
      AttachedHand.EndInteraction(this);
    }

    public virtual void UseButtonUp() {
    }

    public virtual void UseButtonDown() {
    }

    public virtual void AddExternalVelocity(Vector3 velocity) {
      Rigidbody.AddForce(velocity, ForceMode.VelocityChange);
    }

    public virtual void AddExternalAngularVelocity(Vector3 angularVelocity) {
      Rigidbody.AddTorque(angularVelocity, ForceMode.VelocityChange);
    }

    protected virtual void OnDestroy() {
      ForceDetach();
      NVRInteractables.Deregister(this);
    }
  }
}

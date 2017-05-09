using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NewtonVR {
  public class NVRInteractableItem : NVRInteractable {
    private const float MaxVelocityChange = 10f;
    private const float MaxAngularVelocityChange = 20f;
    private const float VelocityMagic = 6000f;
    private const float AngularVelocityMagic = 50f;
    protected Vector3?[] AngularVelocityHistory;
    protected int CurrentVelocityHistoryStep;

    public bool DisablePhysicalMaterialsOnAttach = true;
    protected Vector3 ExternalAngularVelocity;

    protected Vector3 ExternalVelocity;

    [Tooltip(
      "If you have a specific point you'd like the object held at, create a transform there and set it to this variable"
    )] public Transform InteractionPoint;

    protected Dictionary<Collider, PhysicMaterial> MaterialCache =
      new Dictionary<Collider, PhysicMaterial>();

    public UnityEvent OnBeginInteraction;
    public UnityEvent OnEndInteraction;

    public UnityEvent OnHovering;

    public UnityEvent OnUseButtonDown;
    public UnityEvent OnUseButtonUp;

    protected Transform PickupTransform;
    protected float StartingAngularDrag = -1;

    protected float StartingDrag = -1;

    protected Vector3?[] VelocityHistory;

    protected override void Awake() {
      base.Awake();

      Rigidbody.maxAngularVelocity = 100f;
    }

    protected virtual void FixedUpdate() {
      if (IsAttached) {
        bool dropped = CheckForDrop();

        if (dropped == false) {
          UpdateVelocities();
        }
      }

      AddExternalVelocities();
    }

    protected virtual void UpdateVelocities() {
      float velocityMagic = VelocityMagic / (Time.deltaTime / NVRPlayer.NewtonVRExpectedDeltaTime);
      float angularVelocityMagic = AngularVelocityMagic /
                                   (Time.deltaTime / NVRPlayer.NewtonVRExpectedDeltaTime);

      Quaternion rotationDelta;
      Vector3 positionDelta;

      float angle;
      Vector3 axis;

      if (InteractionPoint != null || PickupTransform == null) //PickupTransform should only be null
      {
        rotationDelta = AttachedHand.transform.rotation *
                        Quaternion.Inverse(InteractionPoint.rotation);
        positionDelta = AttachedHand.transform.position - InteractionPoint.position;
      } else {
        rotationDelta = PickupTransform.rotation * Quaternion.Inverse(transform.rotation);
        positionDelta = PickupTransform.position - transform.position;
      }

      rotationDelta.ToAngleAxis(out angle, out axis);

      if (angle > 180) {
        angle -= 360;
      }

      if (angle != 0) {
        Vector3 angularTarget = angle * axis;
        if (float.IsNaN(angularTarget.x) == false) {
          angularTarget = angularTarget * angularVelocityMagic * Time.deltaTime;
          Rigidbody.angularVelocity = Vector3.MoveTowards(Rigidbody.angularVelocity, angularTarget,
            MaxAngularVelocityChange);
        }
      }

      Vector3 velocityTarget = positionDelta * velocityMagic * Time.deltaTime;
      if (float.IsNaN(velocityTarget.x) == false) {
        Rigidbody.velocity = Vector3.MoveTowards(Rigidbody.velocity, velocityTarget,
          MaxVelocityChange);
      }

      if (VelocityHistory != null) {
        CurrentVelocityHistoryStep++;
        if (CurrentVelocityHistoryStep >= VelocityHistory.Length) {
          CurrentVelocityHistoryStep = 0;
        }

        VelocityHistory[CurrentVelocityHistoryStep] = Rigidbody.velocity;
        AngularVelocityHistory[CurrentVelocityHistoryStep] = Rigidbody.angularVelocity;
      }
    }

    protected virtual void AddExternalVelocities() {
      if (ExternalVelocity != Vector3.zero) {
        Rigidbody.velocity = Vector3.Lerp(Rigidbody.velocity, ExternalVelocity, 0.5f);
        ExternalVelocity = Vector3.zero;
      }

      if (ExternalAngularVelocity != Vector3.zero) {
        Rigidbody.angularVelocity = Vector3.Lerp(Rigidbody.angularVelocity, ExternalAngularVelocity,
          0.5f);
        ExternalAngularVelocity = Vector3.zero;
      }
    }

    public override void AddExternalVelocity(Vector3 velocity) {
      if (ExternalVelocity == Vector3.zero) {
        ExternalVelocity = velocity;
      } else {
        ExternalVelocity = Vector3.Lerp(ExternalVelocity, velocity, 0.5f);
      }
    }

    public override void AddExternalAngularVelocity(Vector3 angularVelocity) {
      if (ExternalAngularVelocity == Vector3.zero) {
        ExternalAngularVelocity = angularVelocity;
      } else {
        ExternalAngularVelocity = Vector3.Lerp(ExternalAngularVelocity, angularVelocity, 0.5f);
      }
    }

    public override void BeginInteraction(NVRHand hand) {
      base.BeginInteraction(hand);

      StartingDrag = Rigidbody.drag;
      StartingAngularDrag = Rigidbody.angularDrag;
      Rigidbody.drag = 0;
      Rigidbody.angularDrag = 0.05f;

      if (DisablePhysicalMaterialsOnAttach) {
        DisablePhysicalMaterials();
      }

      PickupTransform =
        new GameObject(string.Format("[{0}] NVRPickupTransform", gameObject.name)).transform;
      PickupTransform.parent = hand.transform;
      PickupTransform.position = transform.position;
      PickupTransform.rotation = transform.rotation;

      ResetVelocityHistory();

      if (OnBeginInteraction != null) {
        OnBeginInteraction.Invoke();
      }
    }

    public override void EndInteraction() {
      base.EndInteraction();

      Rigidbody.drag = StartingDrag;
      Rigidbody.angularDrag = StartingAngularDrag;

      if (PickupTransform != null) {
        Destroy(PickupTransform.gameObject);
      }

      if (DisablePhysicalMaterialsOnAttach) {
        EnablePhysicalMaterials();
      }

      ApplyVelocityHistory();
      ResetVelocityHistory();

      if (OnEndInteraction != null) {
        OnEndInteraction.Invoke();
      }
    }

    public override void HoveringUpdate(NVRHand hand, float forTime) {
      base.HoveringUpdate(hand, forTime);

      if (OnHovering != null) {
        OnHovering.Invoke();
      }
    }

    public override void ResetInteractable() {
      base.ResetInteractable();

      EndInteraction();
    }

    public override void UseButtonDown() {
      base.UseButtonDown();

      if (OnUseButtonDown != null) {
        OnUseButtonDown.Invoke();
      }
    }

    public override void UseButtonUp() {
      base.UseButtonUp();

      if (OnUseButtonUp != null) {
        OnUseButtonUp.Invoke();
      }
    }

    protected virtual void ApplyVelocityHistory() {
      if (VelocityHistory != null) {
        Vector3? meanVelocity = GetMeanVector(VelocityHistory);
        if (meanVelocity != null) {
          Rigidbody.velocity = meanVelocity.Value;
        }

        Vector3? meanAngularVelocity = GetMeanVector(AngularVelocityHistory);
        if (meanAngularVelocity != null) {
          Rigidbody.angularVelocity = meanAngularVelocity.Value;
        }
      }
    }

    protected virtual void ResetVelocityHistory() {
      if (NVRPlayer.Instance.VelocityHistorySteps > 0) {
        CurrentVelocityHistoryStep = 0;

        VelocityHistory = new Vector3?[NVRPlayer.Instance.VelocityHistorySteps];
        AngularVelocityHistory = new Vector3?[NVRPlayer.Instance.VelocityHistorySteps];
      }
    }

    protected Vector3? GetMeanVector(Vector3?[] positions) {
      float x = 0f;
      float y = 0f;
      float z = 0f;

      int count = 0;
      for (int index = 0; index < positions.Length; index++)
        if (positions[index] != null) {
          x += positions[index].Value.x;
          y += positions[index].Value.y;
          z += positions[index].Value.z;

          count++;
        }

      if (count > 0) {
        return new Vector3(x / count, y / count, z / count);
      }

      return null;
    }

    protected void DisablePhysicalMaterials() {
      for (int colliderIndex = 0; colliderIndex < Colliders.Length; colliderIndex++) {
        if (Colliders[colliderIndex] == null) {
          continue;
        }

        MaterialCache[Colliders[colliderIndex]] = Colliders[colliderIndex].sharedMaterial;
        Colliders[colliderIndex].sharedMaterial = null;
      }
    }

    protected void EnablePhysicalMaterials() {
      foreach (Collider c in Colliders) {
        if (c == null) {
          continue;
        }

        if (MaterialCache.ContainsKey(c)) {
          c.sharedMaterial = MaterialCache[c];
        }
      }
    }

    public override void UpdateColliders() {
      base.UpdateColliders();

      if (DisablePhysicalMaterialsOnAttach) {
        for (int colliderIndex = 0; colliderIndex < Colliders.Length; colliderIndex++)
          if (MaterialCache.ContainsKey(Colliders[colliderIndex]) == false) {
            MaterialCache.Add(Colliders[colliderIndex], Colliders[colliderIndex].sharedMaterial);

            if (IsAttached) {
              Colliders[colliderIndex].sharedMaterial = null;
            }
          }
      }
    }
  }
}

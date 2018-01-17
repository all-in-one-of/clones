using System;
using System.Linq;
using UnityEngine;

namespace NewtonVR {
  public class NVRPhysicalController : MonoBehaviour {
    [HideInInspector] public GameObject PhysicalController;
    public bool State;
    protected float AttachedPositionMagic = 3000f;

    protected float AttachedRotationMagic = 20f;

    protected Vector3 ClosestHeldPoint;
    private Collider[] Colliders;
    private NVRHand Hand;

    private readonly Type[] KeepTypes = {typeof(MeshFilter), typeof(Renderer), typeof(Transform), typeof(Rigidbody)};

    private Rigidbody Rigidbody;

    protected float DropDistance {
      get { return 1f; }
    }

    public void Initialize(NVRHand trackingHand, bool initialState) {
      Hand = trackingHand;

      Hand.gameObject.SetActive(false);

      PhysicalController = Instantiate(Hand.gameObject);
      PhysicalController.name = PhysicalController.name.Replace("(Clone)", " [Physical]");

      Hand.gameObject.SetActive(true);

      Component[] components = PhysicalController.GetComponentsInChildren<Component>(true);

      foreach (Component component in components) {
        Type component_type = component.GetType();
        if (KeepTypes.Any(keepType => keepType == component_type || component_type.IsSubclassOf(keepType)) == false) {
          DestroyImmediate(component);
        }
      }

      PhysicalController.transform.parent = Hand.transform.parent;
      PhysicalController.transform.position = Hand.transform.position;
      PhysicalController.transform.rotation = Hand.transform.rotation;
      PhysicalController.transform.localScale = Hand.transform.localScale;

      PhysicalController.SetActive(true);

      if (Hand.HasCustomModel) {
        SetupCustomModel();
      } else {
        Colliders = Hand.SetupDefaultPhysicalColliders(PhysicalController.transform);
      }

      if (Colliders == null) {
        Debug.LogError("[NewtonVR] Error: Physical colliders on hand not setup properly.");
      }

      Rigidbody = PhysicalController.GetComponent<Rigidbody>();
      Rigidbody.isKinematic = false;
      Rigidbody.maxAngularVelocity = float.MaxValue;

      if (trackingHand.Player.AutomaticallySetControllerTransparency) {
        Renderer[] renderers = PhysicalController.GetComponentsInChildren<Renderer>();
        for (int index = 0; index < renderers.Length; index++) {
          NVRHelpers.SetOpaque(renderers[index].material);
        }
      }

      if (initialState == false) {
        Off();
      } else {
        On();
      }
    }

    public void Kill() {
      Destroy(PhysicalController);
      Destroy(this);
    }

    private bool CheckForDrop() {
      float distance = Vector3.Distance(Hand.transform.position, transform.position);

      if (distance > DropDistance) {
        DroppedBecauseOfDistance();
        return true;
      }

      return false;
    }

    private void UpdatePosition() {
      Rigidbody.maxAngularVelocity = float.MaxValue;
      //this doesn't seem to be respected in nvrhand's init. or physical hand's init. not sure why. if anybody knows, let us know. -Keith 6/16/2016

      Quaternion rotationDelta;
      Vector3 positionDelta;

      float angle;
      Vector3 axis;

      rotationDelta = Hand.transform.rotation * Quaternion.Inverse(PhysicalController.transform.rotation);
      positionDelta = (Hand.transform.position - PhysicalController.transform.position);

      rotationDelta.ToAngleAxis(out angle, out axis);

      if (angle > 180) angle -= 360;

      if (angle != 0) {
        Vector3 angularTarget = angle * axis;
        Rigidbody.angularVelocity = angularTarget;
      }

      Vector3 velocityTarget = positionDelta / Time.deltaTime;
      Rigidbody.velocity = velocityTarget;
    }

    protected virtual void FixedUpdate() {
      if (State) {
        bool dropped = CheckForDrop();

        if (dropped == false) {
          UpdatePosition();
        }
      }
    }

    protected virtual void DroppedBecauseOfDistance() {
      Hand.ForceGhost();
    }

    public void On() {
      PhysicalController.transform.position = Hand.transform.position;
      PhysicalController.transform.rotation = Hand.transform.rotation;

      PhysicalController.SetActive(true);

      State = true;
    }

    public void Off() {
      PhysicalController.SetActive(false);

      State = false;
    }

    protected void SetupCustomModel() {
      Transform customCollidersTransform = null;
      if (Hand.CustomPhysicalColliders == null) {
        GameObject customColliders = Instantiate(Hand.CustomModel);
        customColliders.name = "CustomColliders";
        customCollidersTransform = customColliders.transform;

        customCollidersTransform.parent = PhysicalController.transform;
        customCollidersTransform.localPosition = Vector3.zero;
        customCollidersTransform.localRotation = Quaternion.identity;
        customCollidersTransform.localScale = Vector3.one;

        foreach (Collider col in customColliders.GetComponentsInChildren<Collider>()) {
          col.isTrigger = false;
        }

        Colliders = customCollidersTransform.GetComponentsInChildren<Collider>();
      } else {
        GameObject customColliders = Instantiate(Hand.CustomPhysicalColliders);
        customColliders.name = "CustomColliders";
        customCollidersTransform = customColliders.transform;

        customCollidersTransform.parent = PhysicalController.transform;
        customCollidersTransform.localPosition = Vector3.zero;
        customCollidersTransform.localRotation = Quaternion.identity;
        customCollidersTransform.localScale = Hand.CustomPhysicalColliders.transform.localScale;
      }

      Colliders = customCollidersTransform.GetComponentsInChildren<Collider>();
    }
  }
}

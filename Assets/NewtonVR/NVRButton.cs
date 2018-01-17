using UnityEngine;

namespace NewtonVR {
  public class NVRButton : MonoBehaviour {
    [Tooltip("Is set to true when the button has been pressed down this update frame")] public bool ButtonDown;

    [Tooltip("Is set to true each frame the button is pressed down")] public bool ButtonIsPushed;

    [Tooltip("Is set to true when the button has been released from the down position this update frame")] public bool
      ButtonUp;

    [Tooltip("Is set to true if the button was in a pushed state last frame")] public bool ButtonWasPushed;

    [Tooltip(
      "The (worldspace) distance from the initial position you have to push the button for it to register as pushed")] public float DistanceToEngage = 0.075f;

    public Rigidbody Rigidbody;

    protected float CurrentDistance = -1;

    protected Transform InitialPosition;
    protected float MinDistance = 0.001f;

    protected float PositionMagic = 1000f;
    private Vector3 ConstrainedPosition;
    private Quaternion ConstrainedRotation;

    private Vector3 InitialLocalPosition;

    private Quaternion InitialLocalRotation;

    private void Awake() {
      InitialPosition = new GameObject(string.Format("[{0}] Initial Position", gameObject.name)).transform;
      InitialPosition.parent = transform.parent;
      InitialPosition.localPosition = Vector3.zero;
      InitialPosition.localRotation = Quaternion.identity;

      if (Rigidbody == null) Rigidbody = GetComponent<Rigidbody>();

      if (Rigidbody == null) {
        Debug.LogError("There is no rigidbody attached to this button.");
      }

      InitialLocalPosition = transform.localPosition;
      ConstrainedPosition = InitialLocalPosition;

      InitialLocalRotation = transform.localRotation;
      ConstrainedRotation = InitialLocalRotation;
    }

    private void FixedUpdate() {
      ConstrainPosition();

      CurrentDistance = Vector3.Distance(transform.position, InitialPosition.position);

      Vector3 PositionDelta = InitialPosition.position - transform.position;
      Rigidbody.velocity = PositionDelta * PositionMagic * Time.deltaTime;
    }

    private void Update() {
      ButtonWasPushed = ButtonIsPushed;
      ButtonIsPushed = CurrentDistance > DistanceToEngage;

      if (ButtonWasPushed == false && ButtonIsPushed) ButtonDown = true;
      else ButtonDown = false;

      if (ButtonWasPushed && ButtonIsPushed == false) ButtonUp = true;
      else ButtonUp = false;
    }

    private void ConstrainPosition() {
      ConstrainedPosition.y = transform.localPosition.y;
      transform.localPosition = ConstrainedPosition;
      transform.localRotation = ConstrainedRotation;
    }

    private void LateUpdate() {
      ConstrainPosition();
    }
  }
}

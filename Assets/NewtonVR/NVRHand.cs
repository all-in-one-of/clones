using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace NewtonVR {
  public class NVRHand : MonoBehaviour {
    [Serializable]
    public class NVRInteractableEvent : UnityEvent<NVRInteractable> {
    }

    [HideInInspector] public HandState CurrentHandState = HandState.Uninitialized;

    [HideInInspector] public InterationStyle CurrentInteractionStyle;

    public NVRInteractable CurrentlyInteracting;

    [HideInInspector] public GameObject CustomModel;
    [HideInInspector] public GameObject CustomPhysicalColliders;
    public NVRButtons HoldButton = NVRButtons.Grip;

    public Dictionary<NVRButtons, NVRButtonInputs> Inputs;
    [HideInInspector] public bool IsLeft;

    [HideInInspector] public bool IsRight;

    public NVRInteractableEvent OnBeginInteraction = new NVRInteractableEvent();
    public NVRInteractableEvent OnEndInteraction = new NVRInteractableEvent();

    [HideInInspector] public NVRPhysicalController PhysicalController;
    [HideInInspector] public NVRPlayer Player;

    public GameObject RenderModel;

    public Rigidbody Rigidbody;

    public NVRButtons UseButton = NVRButtons.Trigger;

    private Dictionary<NVRInteractable, Dictionary<Collider, float>> CurrentlyHoveringOver;

    private VisibilityLevel CurrentVisibility = VisibilityLevel.Visible;

    private int EstimationSampleIndex;
    private readonly int EstimationSamples = 5;

    private Collider[] GhostColliders;
    private Renderer[] GhostRenderers;

    private NVRInputDevice InputDevice;
    private float[] LastDeltas;
    private Vector3[] LastPositions;
    private Quaternion[] LastRotations;
    private bool VisibilityLocked;

    public bool HoldButtonDown {
      get { return Inputs[HoldButton].PressDown; }
    }

    public bool HoldButtonUp {
      get { return Inputs[HoldButton].PressUp; }
    }

    public bool HoldButtonPressed {
      get { return Inputs[HoldButton].IsPressed; }
    }

    public float HoldButtonAxis {
      get { return Inputs[HoldButton].SingleAxis; }
    }

    public bool UseButtonDown {
      get { return Inputs[UseButton].PressDown; }
    }

    public bool UseButtonUp {
      get { return Inputs[UseButton].PressUp; }
    }

    public bool UseButtonPressed {
      get { return Inputs[UseButton].IsPressed; }
    }

    public float UseButtonAxis {
      get { return Inputs[UseButton].SingleAxis; }
    }

    public bool IsHovering {
      get { return CurrentlyHoveringOver.Any(kvp => kvp.Value.Count > 0); }
    }

    public bool IsInteracting {
      get { return CurrentlyInteracting != null; }
    }

    public bool HasCustomModel {
      get { return CustomModel != null; }
    }

    public bool IsCurrentlyTracked {
      get {
        if (InputDevice != null) {
          return InputDevice.IsCurrentlyTracked;
        }

        return false;
      }
    }

    public Vector3 CurrentForward {
      get {
        if (PhysicalController != null && PhysicalController.State) {
          return PhysicalController.PhysicalController.transform.forward;
        }
        return transform.forward;
      }
    }

    public Vector3 CurrentPosition {
      get {
        if (PhysicalController != null && PhysicalController.State) {
          return PhysicalController.PhysicalController.transform.position;
        }
        return transform.position;
      }
    }

    /// <summary>
    ///   Adds the input device components to this hand to enable tracking for the given integration type.
    /// </summary>
    public void SetupInputDevice(NVRInputDevice input_device) {
      InputDevice = input_device;
      InitializeRenderModel(); // Initializes the render model for this hand from the Input Device.

      Inputs = new Dictionary<NVRButtons, NVRButtonInputs>(new NVRButtonsComparer());
      foreach (NVRButtons button in NVRButtonsHelper.Array) {
        if (Inputs.ContainsKey(button)) {
          throw new ApplicationException("Button already exists in Input dictionary. Duplicate button in helper array.");
        }
        Inputs.Add(button, new NVRButtonInputs(InputDevice, button));
      }
    }

    public virtual void PreInitialize(NVRPlayer player) {
      Player = player;

      IsRight = Player.RightHand == this;
      IsLeft = Player.LeftHand == this;

      CurrentInteractionStyle = Player.InteractionStyle;

      CurrentlyHoveringOver = new Dictionary<NVRInteractable, Dictionary<Collider, float>>();

      LastPositions = new Vector3[EstimationSamples];
      LastRotations = new Quaternion[EstimationSamples];
      LastDeltas = new float[EstimationSamples];
      EstimationSampleIndex = 0;

      VisibilityLocked = false;

      // If we already have an input device attached to this object, use that.
      if (Player.CurrentIntegrationType == NVRSDKIntegrations.Oculus) {
        if (Player.OverrideOculus) {
          if (IsLeft) {
            CustomModel = Player.OverrideOculusLeftHand;
            CustomPhysicalColliders = Player.OverrideOculusLeftHandPhysicalColliders;
          } else if (IsRight) {
            CustomModel = Player.OverrideOculusRightHand;
            CustomPhysicalColliders = Player.OverrideOculusRightHandPhysicalColliders;
          } else {
            Debug.LogError("[NewtonVR] Error: Unknown hand for oculus model override.");
          }
        }
      } else if (Player.CurrentIntegrationType == NVRSDKIntegrations.SteamVR) {
        if (Player.OverrideSteamVR) {
          if (IsLeft) {
            CustomModel = Player.OverrideSteamVRLeftHand;
            CustomPhysicalColliders = Player.OverrideSteamVRLeftHandPhysicalColliders;
          } else if (IsRight) {
            CustomModel = Player.OverrideSteamVRRightHand;
            CustomPhysicalColliders = Player.OverrideSteamVRRightHandPhysicalColliders;
          } else {
            Debug.LogError("[NewtonVR] Error: Unknown hand for SteamVR model override.");
          }
        }
      } else {
        Debug.LogError("[NewtonVR] Error: NVRPlayer.CurrentIntegration not setup.");
        return;
      }

      if (Player.OverrideAll) {
        if (IsLeft) {
          CustomModel = Player.OverrideAllLeftHand;
          CustomPhysicalColliders = Player.OverrideAllLeftHandPhysicalColliders;
        } else if (IsRight) {
          CustomModel = Player.OverrideAllRightHand;
          CustomPhysicalColliders = Player.OverrideAllRightHandPhysicalColliders;
        } else {
          Debug.LogError("[NewtonVR] Error: Unknown hand for SteamVR model override.");
        }
      }
    }

    protected virtual void Update() {
      if (CurrentHandState == HandState.Uninitialized) {
        if (InputDevice == null || InputDevice.ReadyToInitialize() == false) {
          return;
        }
        Initialize();
        return;
      }

      UpdateButtonStates();

      UpdateInteractions();

      UpdateHovering();

      UpdateVisibilityAndColliders();
    }

    protected void UpdateHovering() {
      if (CurrentHandState == HandState.Idle) {
        var hoveringEnumerator = CurrentlyHoveringOver.GetEnumerator();
        while (hoveringEnumerator.MoveNext()) {
          var hoveringOver = hoveringEnumerator.Current;
          if (hoveringOver.Value.Count > 0) {
            hoveringOver.Key.HoveringUpdate(this,
              Time.time - hoveringOver.Value.OrderBy(colliderTime => colliderTime.Value).First().Value);
          }
        }
      }
    }

    protected void UpdateButtonStates() {
      foreach (NVRButtons nvrbutton in NVRButtonsHelper.Array) {
        Inputs[nvrbutton].FrameReset();
      }
    }

    protected void UpdateInteractions() {
      if (CurrentInteractionStyle == InterationStyle.Hold) {
        if (HoldButtonUp) {
          VisibilityLocked = false;
        }

        if (HoldButtonDown) {
          if (CurrentlyInteracting == null) {
            PickupClosest();
          }
        } else if (HoldButtonUp && CurrentlyInteracting != null) {
          EndInteraction(null);
        }
      } else if (CurrentInteractionStyle == InterationStyle.Toggle) {
        if (HoldButtonDown) {
          if (CurrentHandState == HandState.Idle) {
            PickupClosest();
            if (IsInteracting) {
              CurrentHandState = HandState.GripToggleOnInteracting;
            } else if (Player.PhysicalHands) {
              CurrentHandState = HandState.GripToggleOnNotInteracting;
            }
          } else if (CurrentHandState == HandState.GripToggleOnInteracting) {
            CurrentHandState = HandState.Idle;
            VisibilityLocked = false;
            EndInteraction(null);
          } else if (CurrentHandState == HandState.GripToggleOnNotInteracting) {
            CurrentHandState = HandState.Idle;
            VisibilityLocked = false;
          }
        }
      } else if (CurrentInteractionStyle == InterationStyle.ByScript) {
        //this is handled by user customized scripts.
      }

      if (IsInteracting) {
        CurrentlyInteracting.InteractingUpdate(this);
      }
    }

    private void UpdateVisibilityAndColliders() {
      if (Player.PhysicalHands) {
        if (CurrentInteractionStyle == InterationStyle.Hold) {
          if (HoldButtonPressed && IsInteracting == false) {
            if (CurrentHandState != HandState.GripDownNotInteracting && VisibilityLocked == false) {
              VisibilityLocked = true;
              SetVisibility(VisibilityLevel.Visible);
              CurrentHandState = HandState.GripDownNotInteracting;
            }
          } else if (HoldButtonDown && IsInteracting) {
            if (CurrentHandState != HandState.GripDownInteracting && VisibilityLocked == false) {
              VisibilityLocked = true;
              if (Player.MakeControllerInvisibleOnInteraction) {
                SetVisibility(VisibilityLevel.Invisible);
              } else {
                SetVisibility(VisibilityLevel.Ghost);
              }
              CurrentHandState = HandState.GripDownInteracting;
            }
          } else if (IsInteracting == false) {
            if (CurrentHandState != HandState.Idle && VisibilityLocked == false) {
              SetVisibility(VisibilityLevel.Ghost);
              CurrentHandState = HandState.Idle;
            }
          }
        } else if (CurrentInteractionStyle == InterationStyle.Toggle) {
          if (CurrentHandState == HandState.Idle) {
            if (VisibilityLocked == false && CurrentVisibility != VisibilityLevel.Ghost) {
              SetVisibility(VisibilityLevel.Ghost);
            } else {
              VisibilityLocked = false;
            }
          } else if (CurrentHandState == HandState.GripToggleOnInteracting) {
            if (VisibilityLocked == false) {
              VisibilityLocked = true;
              SetVisibility(VisibilityLevel.Ghost);
            }
          } else if (CurrentHandState == HandState.GripToggleOnNotInteracting) {
            if (VisibilityLocked == false) {
              VisibilityLocked = true;
              SetVisibility(VisibilityLevel.Visible);
            }
          }
        }
      } else if (Player.PhysicalHands == false && Player.MakeControllerInvisibleOnInteraction) {
        if (IsInteracting) {
          SetVisibility(VisibilityLevel.Invisible);
        } else if (IsInteracting == false) {
          SetVisibility(VisibilityLevel.Ghost);
        }
      }
    }

    public void TriggerHapticPulse(ushort durationMicroSec = 500, NVRButtons button = NVRButtons.Grip) {
      if (InputDevice != null) {
        if (durationMicroSec < 3000) {
          InputDevice.TriggerHapticPulse(durationMicroSec, button);
        } else {
          Debug.LogWarning(
            "You're trying to pulse for over 3000 microseconds, you probably don't want to do that. If you do, use NVRHand.LongHapticPulse(float seconds)");
        }
      }
    }

    public void LongHapticPulse(float seconds, NVRButtons button = NVRButtons.Grip) {
      StartCoroutine(DoLongHapticPulse(seconds, button));
    }

    private IEnumerator DoLongHapticPulse(float seconds, NVRButtons button) {
      float startTime = Time.time;
      float endTime = startTime + seconds;
      while (Time.time < endTime) {
        TriggerHapticPulse(100, button);
        yield return null;
      }
    }

    public Vector3 GetVelocityEstimation() {
      float delta = LastDeltas.Sum();
      Vector3 distance = Vector3.zero;

      for (int index = 0; index < LastPositions.Length - 1; index++) {
        Vector3 diff = LastPositions[index + 1] - LastPositions[index];
        distance += diff;
      }

      return distance / delta;
    }

    public Vector3 GetAngularVelocityEstimation() {
      float delta = LastDeltas.Sum();
      float angleDegrees = 0.0f;
      Vector3 unitAxis = Vector3.zero;
      Quaternion rotation = Quaternion.identity;

      rotation = LastRotations[LastRotations.Length - 1] * Quaternion.Inverse(LastRotations[LastRotations.Length - 2]);

      //Error: the incorrect rotation is sometimes returned
      rotation.ToAngleAxis(out angleDegrees, out unitAxis);
      return unitAxis * ((angleDegrees * Mathf.Deg2Rad) / delta);
    }

    public Vector3 GetPositionDelta() {
      int last = EstimationSampleIndex - 1;
      int secondToLast = EstimationSampleIndex - 2;

      if (last < 0) last += EstimationSamples;
      if (secondToLast < 0) secondToLast += EstimationSamples;

      return LastPositions[last] - LastPositions[secondToLast];
    }

    public Quaternion GetRotationDelta() {
      int last = EstimationSampleIndex - 1;
      int secondToLast = EstimationSampleIndex - 2;

      if (last < 0) last += EstimationSamples;
      if (secondToLast < 0) secondToLast += EstimationSamples;

      return LastRotations[last] * Quaternion.Inverse(LastRotations[secondToLast]);
    }

    protected virtual void FixedUpdate() {
      if (CurrentHandState == HandState.Uninitialized) {
        return;
      }

      LastPositions[EstimationSampleIndex] = transform.position;
      LastRotations[EstimationSampleIndex] = transform.rotation;
      LastDeltas[EstimationSampleIndex] = Time.deltaTime;
      EstimationSampleIndex++;

      if (EstimationSampleIndex >= LastPositions.Length) EstimationSampleIndex = 0;

      if (InputDevice != null && IsInteracting == false && IsHovering) {
        if (Player.VibrateOnHover) {
          InputDevice.TriggerHapticPulse(100);
        }
      }
    }

    public virtual void BeginInteraction(NVRInteractable interactable) {
      if (interactable.CanAttach) {
        if (interactable.AttachedHand != null) {
          interactable.AttachedHand.EndInteraction(null);
        }

        CurrentlyInteracting = interactable;
        CurrentlyInteracting.BeginInteraction(this);

        if (OnBeginInteraction != null) {
          OnBeginInteraction.Invoke(interactable);
        }
      }
    }

    public virtual void EndInteraction(NVRInteractable item) {
      if (item != null && CurrentlyHoveringOver.ContainsKey(item)) CurrentlyHoveringOver.Remove(item);

      if (CurrentlyInteracting != null) {
        CurrentlyInteracting.EndInteraction();

        if (OnEndInteraction != null) {
          OnEndInteraction.Invoke(CurrentlyInteracting);
        }

        CurrentlyInteracting = null;
      }

      if (CurrentInteractionStyle == InterationStyle.Toggle) {
        CurrentHandState = HandState.Idle;
      }
    }

    private bool PickupClosest() {
      NVRInteractable closest = null;
      float closestDistance = float.MaxValue;

      foreach (var hovering in CurrentlyHoveringOver) {
        if (hovering.Key == null) continue;

        float distance = Vector3.Distance(transform.position, hovering.Key.transform.position);
        if (distance < closestDistance) {
          closestDistance = distance;
          closest = hovering.Key;
        }
      }

      if (closest != null) {
        BeginInteraction(closest);
        return true;
      }
      return false;
    }

    protected virtual void OnTriggerEnter(Collider collider) {
      NVRInteractable interactable = NVRInteractables.GetInteractable(collider);
      if (interactable == null || interactable.enabled == false || collider.tag.Equals("Ungrabbable")) return;

      if (CurrentlyHoveringOver.ContainsKey(interactable) == false) CurrentlyHoveringOver[interactable] = new Dictionary<Collider, float>();

      if (CurrentlyHoveringOver[interactable].ContainsKey(collider) == false) CurrentlyHoveringOver[interactable][collider] = Time.time;
    }

    protected virtual void OnTriggerStay(Collider collider) {
      NVRInteractable interactable = NVRInteractables.GetInteractable(collider);
      if (interactable == null || interactable.enabled == false || collider.tag.Equals("Ungrabbable")) return;

      if (CurrentlyHoveringOver.ContainsKey(interactable) == false) CurrentlyHoveringOver[interactable] = new Dictionary<Collider, float>();

      if (CurrentlyHoveringOver[interactable].ContainsKey(collider) == false) CurrentlyHoveringOver[interactable][collider] = Time.time;
    }

    protected virtual void OnTriggerExit(Collider collider) {
      NVRInteractable interactable = NVRInteractables.GetInteractable(collider);
      if (interactable == null || collider.tag.Equals("Ungrabbable")) return;

      if (CurrentlyHoveringOver.ContainsKey(interactable)) {
        if (CurrentlyHoveringOver[interactable].ContainsKey(collider)) {
          CurrentlyHoveringOver[interactable].Remove(collider);
          if (CurrentlyHoveringOver[interactable].Count == 0) {
            CurrentlyHoveringOver.Remove(interactable);
          }
        }
      }
    }

    public string GetDeviceName() {
      if (InputDevice != null) return InputDevice.GetDeviceName();
      return null;
    }

    public Collider[] SetupDefaultPhysicalColliders(Transform ModelParent) {
      return InputDevice.SetupDefaultPhysicalColliders(ModelParent);
    }

    public void DeregisterInteractable(NVRInteractable interactable) {
      if (CurrentlyInteracting == interactable) CurrentlyInteracting = null;

      if (CurrentlyHoveringOver != null && CurrentlyHoveringOver.ContainsKey(interactable)) CurrentlyHoveringOver.Remove(interactable);
    }

    private void SetVisibility(VisibilityLevel visibility) {
      if (CurrentVisibility != visibility) {
        if (visibility == VisibilityLevel.Invisible) {
          if (PhysicalController != null) {
            PhysicalController.Off();
          }

          if (Player.AutomaticallySetControllerTransparency) {
            for (int index = 0; index < GhostRenderers.Length; index++) {
              GhostRenderers[index].enabled = false;
            }

            for (int index = 0; index < GhostColliders.Length; index++) {
              GhostColliders[index].enabled = false;
            }
          }
        }

        if (visibility == VisibilityLevel.Ghost) {
          if (PhysicalController != null) {
            PhysicalController.Off();
          }

          if (Player.AutomaticallySetControllerTransparency) {
            for (int index = 0; index < GhostRenderers.Length; index++) {
              GhostRenderers[index].enabled = true;
            }

            for (int index = 0; index < GhostColliders.Length; index++) {
              GhostColliders[index].enabled = true;
            }
          }
        }

        if (visibility == VisibilityLevel.Visible) {
          if (PhysicalController != null) {
            PhysicalController.On();
          }

          if (Player.AutomaticallySetControllerTransparency) {
            for (int index = 0; index < GhostRenderers.Length; index++) {
              GhostRenderers[index].enabled = false;
            }

            for (int index = 0; index < GhostColliders.Length; index++) {
              GhostColliders[index].enabled = false;
            }
          }
        }
      }

      CurrentVisibility = visibility;
    }

    protected void InitializeRenderModel() {
      if (CustomModel == null) {
        RenderModel = InputDevice.SetupDefaultRenderModel().ValueOr((GameObject) null);
      } else {
        GameObject RenderModel = Instantiate(CustomModel);

        RenderModel.transform.parent = transform;
        RenderModel.transform.localScale = RenderModel.transform.localScale;
        RenderModel.transform.localPosition = Vector3.zero;
        RenderModel.transform.localRotation = Quaternion.identity;
      }
    }

    public void Initialize() {
      Rigidbody = GetComponent<Rigidbody>();
      if (Rigidbody == null) Rigidbody = gameObject.AddComponent<Rigidbody>();
      Rigidbody.isKinematic = true;
      Rigidbody.maxAngularVelocity = float.MaxValue;
      Rigidbody.useGravity = false;

      Collider[] colliders = null;

      if (CustomModel == null) {
        colliders = InputDevice.SetupDefaultColliders();
      } else {
        //note: these should be trigger colliders
        colliders = new Collider[] {};
        if (RenderModel != null) {
          colliders = RenderModel.GetComponentsInChildren<Collider>();
        }
      }

      Player.RegisterHand(this);

      if (Player.PhysicalHands) {
        if (PhysicalController != null) {
          PhysicalController.Kill();
        }

        PhysicalController = gameObject.AddComponent<NVRPhysicalController>();
        PhysicalController.Initialize(this, false);

        if (Player.AutomaticallySetControllerTransparency) {
          Color transparentcolor = Color.white;
          transparentcolor.a = (float) VisibilityLevel.Ghost / 100f;

          GhostRenderers = GetComponentsInChildren<Renderer>();
          for (int rendererIndex = 0; rendererIndex < GhostRenderers.Length; rendererIndex++) {
            NVRHelpers.SetTransparent(GhostRenderers[rendererIndex].material, transparentcolor);
          }
        }

        if (colliders != null) {
          GhostColliders = colliders;
        }

        CurrentVisibility = VisibilityLevel.Ghost;
      } else {
        if (Player.AutomaticallySetControllerTransparency) {
          Color transparentcolor = Color.white;
          transparentcolor.a = (float) VisibilityLevel.Ghost / 100f;

          GhostRenderers = GetComponentsInChildren<Renderer>();
          for (int rendererIndex = 0; rendererIndex < GhostRenderers.Length; rendererIndex++) {
            NVRHelpers.SetTransparent(GhostRenderers[rendererIndex].material, transparentcolor);
          }
        }

        if (colliders != null) {
          GhostColliders = colliders;
        }

        CurrentVisibility = VisibilityLevel.Ghost;
      }

      CurrentHandState = HandState.Idle;
    }

    public void ForceGhost() {
      SetVisibility(VisibilityLevel.Ghost);
      PhysicalController.Off();
    }

    public void OnDestroy() {
      if (PhysicalController != null) {
        Destroy(PhysicalController.gameObject);
      }
    }
  }

  public enum VisibilityLevel {
    Invisible = 0,
    Ghost = 70,
    Visible = 100
  }

  public enum HandState {
    Uninitialized,
    Idle,
    GripDownNotInteracting,
    GripDownInteracting,
    GripToggleOnNotInteracting,
    GripToggleOnInteracting,
    GripToggleOff
  }

  public enum InterationStyle {
    Hold,
    Toggle,
    ByScript
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VR;

namespace NewtonVR {
  public class NVRPlayer : MonoBehaviour {
    public const decimal NewtonVRVersion = 1.191m;
    public const float NewtonVRExpectedDeltaTime = 0.0111f;

    public static List<NVRPlayer> Instances = new List<NVRPlayer>();
    public bool AutomaticallySetControllerTransparency = true;

    public bool AutoSetFixedDeltaTime = true;

    private Dictionary<Collider, NVRHand> ColliderToHandMapping;

    [HideInInspector] public NVRSDKIntegrations CurrentIntegrationType = NVRSDKIntegrations.None;
    public bool DEBUGDropFrames = false;

    [Space] public bool DEBUGEnableFallback2D = false;
    public int DEBUGSleepPerFrame = 13;
    public Mesh EditorPlayerPreview;
    public Vector2 EditorPlayspaceDefault = new Vector2(2, 1.5f);
    public bool EditorPlayspaceOverride = false;
    public Mesh EditorPlayspacePreview;

    [Space] public bool EnableEditorPlayerPreview = true;
    public NVRHand FakeHand;
    public NVRHand FakeHand2;

    [HideInInspector] public List<NVRHand> Hands = new List<NVRHand>();

    [Space] public NVRHead Head;

    private NVRIntegration Integration;

    public InterationStyle InteractionStyle;
    public NVRHand LeftHand;
    public bool MakeControllerInvisibleOnInteraction = false;
    public bool NotifyOnVersionUpdate = true;
    [HideInInspector] public bool OculusSDKEnabled = false;

    public UnityEvent OnInitialized;

    [Space] [HideInInspector] public bool OverrideAll;
    [HideInInspector] public GameObject OverrideAllLeftHand;
    [HideInInspector] public GameObject OverrideAllLeftHandPhysicalColliders;
    [HideInInspector] public GameObject OverrideAllRightHand;
    [HideInInspector] public GameObject OverrideAllRightHandPhysicalColliders;

    [HideInInspector] public bool OverrideOculus;
    [HideInInspector] public GameObject OverrideOculusLeftHand;
    [HideInInspector] public GameObject OverrideOculusLeftHandPhysicalColliders;
    [HideInInspector] public GameObject OverrideOculusRightHand;
    [HideInInspector] public GameObject OverrideOculusRightHandPhysicalColliders;

    [HideInInspector] public bool OverrideSteamVR;
    [HideInInspector] public GameObject OverrideSteamVRLeftHand;
    [HideInInspector] public GameObject OverrideSteamVRLeftHandPhysicalColliders;
    [HideInInspector] public GameObject OverrideSteamVRRightHand;
    [HideInInspector] public GameObject OverrideSteamVRRightHandPhysicalColliders;
    public bool PhysicalHands = true;
    public NVRHand RightHand;

    [HideInInspector] public bool SteamVREnabled = false;
    public int VelocityHistorySteps = 3;
    public bool VibrateOnHover = true;

    public static NVRPlayer Instance {
      get { return Instances.First(player => player != null && player.gameObject != null); }
    }

    public Vector3 PlayspaceSize {
      get {
#if !UNITY_5_5_OR_NEWER
                if (Application.isPlaying == false)
                {
                    return Vector3.zero; //not supported in unity below 5.5.
                }
#endif

        if (Integration != null) {
          return Integration.GetPlayspaceBounds();
        }
        if (OculusSDKEnabled) {
          Integration = new NVROculusIntegration();
          if (Integration.IsHmdPresent()) {
            return Integration.GetPlayspaceBounds();
          }
          Integration = null;
        }

        if (SteamVREnabled) {
          Integration = new NVRSteamVRIntegration();
          if (Integration.IsHmdPresent()) {
            return Integration.GetPlayspaceBounds();
          }
          Integration = null;
        }

        return Vector3.zero;
      }
    }

    private void Awake() {
      if (AutoSetFixedDeltaTime) {
        Time.fixedDeltaTime = NewtonVRExpectedDeltaTime;
      }

      Instances.Add(this);

      NVRInteractables.Initialize();

      if (Head == null) {
        Head = GetComponentInChildren<NVRHead>();
      }
      Head.Initialize();

      if (LeftHand == null || RightHand == null) {
        Debug.LogError("[FATAL ERROR] Please set the left and right hand to a nvrhands.");
      }

      ColliderToHandMapping = new Dictionary<Collider, NVRHand>();

      SetupIntegration();

      if (Hands.Count == 0) {
        Hands = new List<NVRHand> {LeftHand, RightHand};

        foreach (NVRHand hand in Hands) {
          hand.PreInitialize(this);
          NVRInputDevice dev = null;
          switch (CurrentIntegrationType) {
            case NVRSDKIntegrations.Oculus:
              dev = hand.gameObject.AddComponent<NVROculusInputDevice>();
              break;
            case NVRSDKIntegrations.SteamVR:
              dev = hand.gameObject.AddComponent<NVRSteamVRInputDevice>();
              break;
            case NVRSDKIntegrations.FallbackNonVR:
            case NVRSDKIntegrations.None:
            default:
              Debug.LogError("[NewtonVR] Error: NVRPlayer.CurrentIntegration not setup.");
              break;
          }

          dev.Initialize(hand);
          hand.SetupInputDevice(dev);
        }
      }

      if (Integration != null) {
        Integration.Initialize(this);
      }

      if (OnInitialized != null) {
        OnInitialized.Invoke();
      }
    }

    private void SetupIntegration(bool logOutput = true) {
      CurrentIntegrationType = DetermineCurrentIntegration(logOutput);

      if (CurrentIntegrationType == NVRSDKIntegrations.Oculus) {
        Integration = new NVROculusIntegration();
      } else if (CurrentIntegrationType == NVRSDKIntegrations.SteamVR) {
        Integration = new NVRSteamVRIntegration();
      } else if (CurrentIntegrationType == NVRSDKIntegrations.FallbackNonVR) {
        if (logOutput) {
          Debug.LogError("[NewtonVR] Fallback non-vr not yet implemented.");
        }
      } else {
        if (logOutput) {
          Debug.LogError(
            "[NewtonVR] Critical Error: Oculus / SteamVR not setup properly or no headset found.");
        }
      }
    }

    private NVRSDKIntegrations DetermineCurrentIntegration(bool logOutput = true) {
      NVRSDKIntegrations currentIntegration = NVRSDKIntegrations.None;
      string resultLog = "[NewtonVR] Version : " + NewtonVRVersion + ". ";

      if (VRDevice.isPresent) {
        resultLog += "Found VRDevice: " + VRDevice.model + ". ";

#if !NVR_Oculus && !NVR_SteamVR
                string warning = "Neither SteamVR or Oculus SDK is enabled in the NVRPlayer. Please check the \"Enable SteamVR\" or \"Enable Oculus SDK\" checkbox in the NVRPlayer script in the NVRPlayer GameObject.";
                Debug.LogWarning(warning);
#endif

#if NVR_Oculus
                if (VRDevice.model.IndexOf("oculus", System.StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    currentIntegration = NVRSDKIntegrations.Oculus;
                    resultLog += "Using Oculus SDK";
                }
#endif

#if NVR_SteamVR
        if (currentIntegration == NVRSDKIntegrations.None) {
          currentIntegration = NVRSDKIntegrations.SteamVR;
          resultLog += "Using SteamVR SDK";
        }
#endif
      }

      if (currentIntegration == NVRSDKIntegrations.None) {
        if (DEBUGEnableFallback2D) {
          currentIntegration = NVRSDKIntegrations.FallbackNonVR;
        } else {
          resultLog += "Did not find supported VR device. Or no integrations enabled.";
        }
      }

      if (logOutput) {
        Debug.Log(resultLog);
      }

      return currentIntegration;
    }

    public void RegisterHand(NVRHand hand) {
      Collider[] colliders = hand.GetComponentsInChildren<Collider>();

      for (int index = 0; index < colliders.Length; index++)
        if (ColliderToHandMapping.ContainsKey(colliders[index]) == false) {
          ColliderToHandMapping.Add(colliders[index], hand);
        }
    }

    public NVRHand GetHand(Collider collider) {
      return ColliderToHandMapping[collider];
    }

    public static void DeregisterInteractable(NVRInteractable interactable) {
      foreach (NVRPlayer player in Instances) {
        if (player == null || player.Hands == null) {
          continue;
        }
        foreach (NVRHand hand in player.Hands)
          if (hand != null) {
            hand.DeregisterInteractable(interactable);
          }
      }
    }

    private void OnDestroy() {
      Instances.Remove(this);
    }

    private void Update() {
      if (DEBUGDropFrames) {
        Thread.Sleep(DEBUGSleepPerFrame);
      }
    }

#if UNITY_EDITOR
    private static DateTime LastRequestedSize;
    private static Vector3 CachedPlayspaceScale;

    private void OnDrawGizmos() {
      if (EnableEditorPlayerPreview == false) {
        return;
      }

      if (Application.isPlaying) {
        return;
      }

      TimeSpan lastRequested = DateTime.Now - LastRequestedSize;
      Vector3 playspaceScale;
      if (lastRequested.TotalSeconds > 1) {
        if (EditorPlayspaceOverride == false) {
          Vector3 returnedPlayspaceSize = PlayspaceSize;
          if (returnedPlayspaceSize == Vector3.zero) {
            playspaceScale = EditorPlayspaceDefault;
            playspaceScale.z = playspaceScale.y;
          } else {
            playspaceScale = returnedPlayspaceSize;
          }
        } else {
          playspaceScale = EditorPlayspaceDefault;
          playspaceScale.z = playspaceScale.y;
        }

        playspaceScale.y = 1f;
        LastRequestedSize = DateTime.Now;
      } else {
        playspaceScale = CachedPlayspaceScale;
      }
      CachedPlayspaceScale = playspaceScale;

      Color drawColor = Color.green;
      drawColor.a = 0.075f;
      Gizmos.color = drawColor;
      Gizmos.DrawWireMesh(EditorPlayerPreview, transform.position, transform.rotation,
        transform.localScale);
      drawColor.a = 0.5f;
      Gizmos.color = drawColor;
      Gizmos.DrawWireMesh(EditorPlayspacePreview, transform.position, transform.rotation,
        playspaceScale * transform.localScale.x);
    }
#endif
  }
}

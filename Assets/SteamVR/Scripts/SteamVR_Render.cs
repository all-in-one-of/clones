//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Handles rendering of all SteamVR_Cameras
//
//=============================================================================

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Valve.VR;

public class SteamVR_Render : MonoBehaviour {
  private static SteamVR_Render _instance;

  private static bool isQuitting;

  private static bool _pauseRendering;

  public SteamVR_ExternalCamera externalCamera;
  public string externalCameraConfigPath = "externalcamera.cfg";
  public TrackedDevicePose_t[] gamePoses = new TrackedDevicePose_t[0];
  public bool lockPhysicsUpdateRateToRenderFrequency = true;
  public bool pauseGameWhenDashboardIsVisible = true;

  public TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

  public ETrackingUniverseOrigin trackingSpace = ETrackingUniverseOrigin.TrackingUniverseStanding;

  private SteamVR_Camera[] cameras = new SteamVR_Camera[0];

#if !(UNITY_5_6)
  private SteamVR_UpdatePoses poseUpdater;
#endif

  private float sceneResolutionScale = 1.0f, timeScale = 1.0f;

  private readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

  public static EVREye eye { get; private set; }

  public static SteamVR_Render instance {
    get {
      if (_instance == null) {
        _instance = FindObjectOfType<SteamVR_Render>();

        if (_instance == null) _instance = new GameObject("[SteamVR]").AddComponent<SteamVR_Render>();
      }
      return _instance;
    }
  }

  public static bool pauseRendering {
    get { return _pauseRendering; }
    set {
      _pauseRendering = value;

      var compositor = OpenVR.Compositor;
      if (compositor != null) compositor.SuspendRendering(value);
    }
  }

  private void OnDestroy() {
    _instance = null;
  }

  private void OnApplicationQuit() {
    isQuitting = true;
    SteamVR.SafeDispose();
  }

  public static void Add(SteamVR_Camera vrcam) {
    if (!isQuitting) instance.AddInternal(vrcam);
  }

  public static void Remove(SteamVR_Camera vrcam) {
    if (!isQuitting && _instance != null) instance.RemoveInternal(vrcam);
  }

  public static SteamVR_Camera Top() {
    if (!isQuitting) return instance.TopInternal();

    return null;
  }

  private void AddInternal(SteamVR_Camera vrcam) {
    var camera = vrcam.GetComponent<Camera>();
    var length = cameras.Length;
    var sorted = new SteamVR_Camera[length + 1];
    int insert = 0;
    for (int i = 0; i < length; i++) {
      var c = cameras[i].GetComponent<Camera>();
      if (i == insert && c.depth > camera.depth) sorted[insert++] = vrcam;

      sorted[insert++] = cameras[i];
    }
    if (insert == length) sorted[insert] = vrcam;

    cameras = sorted;
  }

  private void RemoveInternal(SteamVR_Camera vrcam) {
    var length = cameras.Length;
    int count = 0;
    for (int i = 0; i < length; i++) {
      var c = cameras[i];
      if (c == vrcam) ++count;
    }
    if (count == 0) return;

    var sorted = new SteamVR_Camera[length - count];
    int insert = 0;
    for (int i = 0; i < length; i++) {
      var c = cameras[i];
      if (c != vrcam) sorted[insert++] = c;
    }

    cameras = sorted;
  }

  private SteamVR_Camera TopInternal() {
    if (cameras.Length > 0) return cameras[cameras.Length - 1];

    return null;
  }

  private IEnumerator RenderLoop() {
    while (Application.isPlaying) {
      yield return waitForEndOfFrame;

      if (pauseRendering) continue;

      var compositor = OpenVR.Compositor;
      if (compositor != null) {
        if (!compositor.CanRenderScene()) continue;

        compositor.SetTrackingSpace(trackingSpace);
      }

      var overlay = SteamVR_Overlay.instance;
      if (overlay != null) overlay.UpdateOverlay();

      RenderExternalCamera();
    }
  }

  private void RenderExternalCamera() {
    if (externalCamera == null) return;

    if (!externalCamera.gameObject.activeInHierarchy) return;

    var frameSkip = (int) Mathf.Max(externalCamera.config.frameSkip, 0.0f);
    if (Time.frameCount % (frameSkip + 1) != 0) return;

    // Keep external camera relative to the most relevant vr camera.
    externalCamera.AttachToCamera(TopInternal());

    externalCamera.RenderNear();
    externalCamera.RenderFar();
  }

  private void OnInputFocus(bool hasFocus) {
    if (hasFocus) {
      if (pauseGameWhenDashboardIsVisible) {
        Time.timeScale = timeScale;
      }

      SteamVR_Camera.sceneResolutionScale = sceneResolutionScale;
    } else {
      if (pauseGameWhenDashboardIsVisible) {
        timeScale = Time.timeScale;
        Time.timeScale = 0.0f;
      }

      sceneResolutionScale = SteamVR_Camera.sceneResolutionScale;
      SteamVR_Camera.sceneResolutionScale = 0.5f;
    }
  }

  private void OnQuit(VREvent_t vrEvent) {
#if UNITY_EDITOR
    foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
      var t = a.GetType("UnityEditor.EditorApplication");
      if (t != null) {
        t.GetProperty("isPlaying").SetValue(null, false, null);
        break;
      }
    }
#else
		Application.Quit();
#endif
  }

  private string GetScreenshotFilename(uint screenshotHandle, EVRScreenshotPropertyFilenames screenshotPropertyFilename) {
    var error = EVRScreenshotError.None;
    var capacity = OpenVR.Screenshots.GetScreenshotPropertyFilename(screenshotHandle, screenshotPropertyFilename, null,
      0, ref error);
    if (error != EVRScreenshotError.None && error != EVRScreenshotError.BufferTooSmall) return null;
    if (capacity > 1) {
      var result = new StringBuilder((int) capacity);
      OpenVR.Screenshots.GetScreenshotPropertyFilename(screenshotHandle, screenshotPropertyFilename, result, capacity,
        ref error);
      if (error != EVRScreenshotError.None) return null;
      return result.ToString();
    }
    return null;
  }

  private void OnRequestScreenshot(VREvent_t vrEvent) {
    var screenshotHandle = vrEvent.data.screenshot.handle;
    var screenshotType = (EVRScreenshotType) vrEvent.data.screenshot.type;

    if (screenshotType == EVRScreenshotType.StereoPanorama) {
      string previewFilename = GetScreenshotFilename(screenshotHandle, EVRScreenshotPropertyFilenames.Preview);
      string VRFilename = GetScreenshotFilename(screenshotHandle, EVRScreenshotPropertyFilenames.VR);

      if (previewFilename == null || VRFilename == null) return;

      // Do the stereo panorama screenshot
      // Figure out where the view is
      GameObject screenshotPosition = new GameObject("screenshotPosition");
      screenshotPosition.transform.position = Top().transform.position;
      screenshotPosition.transform.rotation = Top().transform.rotation;
      screenshotPosition.transform.localScale = Top().transform.lossyScale;
      SteamVR_Utils.TakeStereoScreenshot(screenshotHandle, screenshotPosition, 32, 0.064f, ref previewFilename,
        ref VRFilename);

      // and submit it
      OpenVR.Screenshots.SubmitScreenshot(screenshotHandle, screenshotType, previewFilename, VRFilename);
    }
  }

  private void OnEnable() {
    StartCoroutine("RenderLoop");
    SteamVR_Events.InputFocus.Listen(OnInputFocus);
    SteamVR_Events.System(EVREventType.VREvent_Quit).Listen(OnQuit);
    SteamVR_Events.System(EVREventType.VREvent_RequestScreenshot).Listen(OnRequestScreenshot);

    var vr = SteamVR.instance;
    if (vr == null) {
      enabled = false;
      return;
    }
    var types = new[] {EVRScreenshotType.StereoPanorama};
    OpenVR.Screenshots.HookScreenshot(types);
  }

  private void OnDisable() {
    StopAllCoroutines();
    SteamVR_Events.InputFocus.Remove(OnInputFocus);
    SteamVR_Events.System(EVREventType.VREvent_Quit).Remove(OnQuit);
    SteamVR_Events.System(EVREventType.VREvent_RequestScreenshot).Remove(OnRequestScreenshot);
  }

  private void Awake() {
    if (externalCamera == null && File.Exists(externalCameraConfigPath)) {
      var prefab = Resources.Load<GameObject>("SteamVR_ExternalCamera");
      var instance = Instantiate(prefab);
      instance.gameObject.name = "External Camera";

      externalCamera = instance.transform.GetChild(0).GetComponent<SteamVR_ExternalCamera>();
      externalCamera.configPath = externalCameraConfigPath;
      externalCamera.ReadConfig();
    }
  }

  private void Update() {
#if !(UNITY_5_6)
    if (poseUpdater == null) {
      var go = new GameObject("poseUpdater");
      go.transform.parent = transform;
      poseUpdater = go.AddComponent<SteamVR_UpdatePoses>();
    }
#endif
    // Force controller update in case no one else called this frame to ensure prevState gets updated.
    SteamVR_Controller.Update();

    // Dispatch any OpenVR events.
    var system = OpenVR.System;
    if (system != null) {
      var vrEvent = new VREvent_t();
      var size = (uint) Marshal.SizeOf(typeof(VREvent_t));
      for (int i = 0; i < 64; i++) {
        if (!system.PollNextEvent(ref vrEvent, size)) break;

        switch ((EVREventType) vrEvent.eventType) {
          case EVREventType.VREvent_InputFocusCaptured:
            // another app has taken focus (likely dashboard)
            if (vrEvent.data.process.oldPid == 0) {
              SteamVR_Events.InputFocus.Send(false);
            }
            break;
          case EVREventType.VREvent_InputFocusReleased: // that app has released input focus
            if (vrEvent.data.process.pid == 0) {
              SteamVR_Events.InputFocus.Send(true);
            }
            break;
          case EVREventType.VREvent_ShowRenderModels:
            SteamVR_Events.HideRenderModels.Send(false);
            break;
          case EVREventType.VREvent_HideRenderModels:
            SteamVR_Events.HideRenderModels.Send(true);
            break;
          default:
            SteamVR_Events.System((EVREventType) vrEvent.eventType).Send(vrEvent);
            break;
        }
      }
    }

    // Ensure various settings to minimize latency.
    Application.targetFrameRate = -1;
    Application.runInBackground = true; // don't require companion window focus
    QualitySettings.maxQueuedFrames = -1;
    QualitySettings.vSyncCount = 0; // this applies to the companion window

    if (lockPhysicsUpdateRateToRenderFrequency && Time.timeScale > 0.0f) {
      var vr = SteamVR.instance;
      if (vr != null) {
        var timing = new Compositor_FrameTiming();
        timing.m_nSize = (uint) Marshal.SizeOf(typeof(Compositor_FrameTiming));
        vr.compositor.GetFrameTiming(ref timing, 0);

        Time.fixedDeltaTime = Time.timeScale / vr.hmd_DisplayFrequency;
      }
    }
  }
}

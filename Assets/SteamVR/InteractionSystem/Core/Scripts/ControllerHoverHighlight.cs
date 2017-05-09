﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Highlights the controller when hovering over interactables
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem {
  //-------------------------------------------------------------------------
  public class ControllerHoverHighlight : MonoBehaviour {
    private MeshRenderer bodyMeshRenderer;
    public bool fireHapticsOnHightlight = true;

    private Hand hand;
    public Material highLightMaterial;
    private SteamVR_RenderModel renderModel;
    private bool renderModelLoaded;

    private SteamVR_Events.Action renderModelLoadedAction;
    private MeshRenderer trackingHatMeshRenderer;

    //-------------------------------------------------
    private void Start() {
      hand = GetComponentInParent<Hand>();
    }

    //-------------------------------------------------
    private void Awake() {
      renderModelLoadedAction = SteamVR_Events.RenderModelLoadedAction(OnRenderModelLoaded);
    }

    //-------------------------------------------------
    private void OnEnable() {
      renderModelLoadedAction.enabled = true;
    }

    //-------------------------------------------------
    private void OnDisable() {
      renderModelLoadedAction.enabled = false;
    }

    //-------------------------------------------------
    private void OnHandInitialized(int deviceIndex) {
      renderModel = gameObject.AddComponent<SteamVR_RenderModel>();
      renderModel.SetDeviceIndex(deviceIndex);
      renderModel.updateDynamically = false;
    }

    //-------------------------------------------------
    private void OnRenderModelLoaded(SteamVR_RenderModel renderModel, bool success) {
      if (renderModel != this.renderModel) {
        return;
      }

      Transform bodyTransform = transform.Find("body");
      if (bodyTransform != null) {
        bodyMeshRenderer = bodyTransform.GetComponent<MeshRenderer>();
        bodyMeshRenderer.material = highLightMaterial;
        bodyMeshRenderer.enabled = false;
      }

      Transform trackingHatTransform = transform.Find("trackhat");
      if (trackingHatTransform != null) {
        trackingHatMeshRenderer = trackingHatTransform.GetComponent<MeshRenderer>();
        trackingHatMeshRenderer.material = highLightMaterial;
        trackingHatMeshRenderer.enabled = false;
      }

      foreach (Transform child in transform)
        if (child.name != "body" && child.name != "trackhat") {
          Destroy(child.gameObject);
        }

      renderModelLoaded = true;
    }

    //-------------------------------------------------
    private void OnParentHandHoverBegin(Interactable other) {
      if (!isActiveAndEnabled) {
        return;
      }

      if (other.transform.parent != transform.parent) {
        ShowHighlight();
      }
    }

    //-------------------------------------------------
    private void OnParentHandHoverEnd(Interactable other) {
      HideHighlight();
    }

    //-------------------------------------------------
    private void OnParentHandInputFocusAcquired() {
      if (!isActiveAndEnabled) {
        return;
      }

      if (hand.hoveringInteractable &&
          hand.hoveringInteractable.transform.parent != transform.parent) {
        ShowHighlight();
      }
    }

    //-------------------------------------------------
    private void OnParentHandInputFocusLost() {
      HideHighlight();
    }

    //-------------------------------------------------
    public void ShowHighlight() {
      if (renderModelLoaded == false) {
        return;
      }

      if (fireHapticsOnHightlight) {
        hand.controller.TriggerHapticPulse(500);
      }

      if (bodyMeshRenderer != null) {
        bodyMeshRenderer.enabled = true;
      }

      if (trackingHatMeshRenderer != null) {
        trackingHatMeshRenderer.enabled = true;
      }
    }

    //-------------------------------------------------
    public void HideHighlight() {
      if (renderModelLoaded == false) {
        return;
      }

      if (fireHapticsOnHightlight) {
        hand.controller.TriggerHapticPulse(300);
      }

      if (bodyMeshRenderer != null) {
        bodyMeshRenderer.enabled = false;
      }

      if (trackingHatMeshRenderer != null) {
        trackingHatMeshRenderer.enabled = false;
      }
    }
  }
}

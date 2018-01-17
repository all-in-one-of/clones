//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Changes the pitch of this audio source based on a linear mapping
//			and a curve
//
//=============================================================================

using UnityEngine;

namespace Valve.VR.InteractionSystem {
  //-------------------------------------------------------------------------
  public class LinearAudioPitch : MonoBehaviour {
    public bool applyContinuously = true;
    public LinearMapping linearMapping;
    public float maxPitch;
    public float minPitch;
    public AnimationCurve pitchCurve;

    private AudioSource audioSource;

    //-------------------------------------------------
    private void Awake() {
      if (audioSource == null) {
        audioSource = GetComponent<AudioSource>();
      }

      if (linearMapping == null) {
        linearMapping = GetComponent<LinearMapping>();
      }
    }

    //-------------------------------------------------
    private void Update() {
      if (applyContinuously) {
        Apply();
      }
    }

    //-------------------------------------------------
    private void Apply() {
      float y = pitchCurve.Evaluate(linearMapping.value);

      audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, y);
    }
  }
}

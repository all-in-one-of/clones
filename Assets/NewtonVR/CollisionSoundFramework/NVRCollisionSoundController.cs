using UnityEngine;

namespace NewtonVR {
  public class NVRCollisionSoundController : MonoBehaviour {
    public static NVRCollisionSoundController Instance;

    private static NVRCollisionSoundProvider Provider;

    public float MaxCollisionVelocity = 5;

    [Tooltip(
      "Don't play collision sounds that will produce an impact with a volume lower than this number"
    )] public float MinCollisionVolume = 0.1f;

    [Tooltip("Turns on or off randomizing the pitch of the collision sounds")] public bool
      PitchModulationEnabled = true;

    [Range(0f, 3f)] public float PitchModulationRange = 0.5f;

    [HideInInspector] public NVRCollisionSoundProviders SoundEngine =
      NVRCollisionSoundProviders.Unity;

    [Tooltip("The max number of sounds that can possibly be playing at once.")] public int
      SoundPoolSize = 100;

    private void Awake() {
      Instance = this;

#if NVR_FMOD
            Provider = this.gameObject.AddComponent<NVRCollisionSoundProviderFMOD>();
            #else
      Provider = gameObject.AddComponent<NVRCollisionSoundProviderUnity>();
#endif
    }

    public static void Play(NVRCollisionSoundMaterials material, Vector3 position,
                            float impactVolume) {
      if (Provider != null) {
        Provider.Play(material, position, impactVolume);
      }
    }
  }

  public enum NVRCollisionSoundProviders {
    None,
    Unity,
    FMOD
  }
}

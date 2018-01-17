using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewtonVR {
  public class NVRInteractables : MonoBehaviour {
    private static Dictionary<Collider, NVRInteractable> ColliderMapping;
    private static Dictionary<NVRInteractable, Collider[]> NVRInteractableMapping;

    private static bool Initialized;

    public static void Initialize() {
      ColliderMapping = new Dictionary<Collider, NVRInteractable>();
      NVRInteractableMapping = new Dictionary<NVRInteractable, Collider[]>();

      Initialized = true;
    }

    public static void Register(NVRInteractable interactable, Collider[] colliders) {
      if (Initialized == false) {
        Debug.LogError("[NewtonVR] Error: NVRInteractables.Register called before initialization.");
      }

      NVRInteractableMapping[interactable] = colliders;

      foreach (Collider collider in colliders) {
        ColliderMapping[collider] = interactable;
      }

      Debug.Log("Registered new collider. Total registered: " + ColliderMapping.Count);
    }

    public static void Deregister(NVRInteractable interactable) {
      if (Initialized == false) {
        Debug.LogError("[NewtonVR] Error: NVRInteractables.Register called before initialization.");
      }

      NVRPlayer.DeregisterInteractable(interactable);

      ColliderMapping =
        ColliderMapping.Where(mapping => mapping.Value != interactable)
                       .ToDictionary(mapping => mapping.Key, mapping => mapping.Value);
      NVRInteractableMapping.Remove(interactable);
    }

    public static NVRInteractable GetInteractable(Collider collider) {
      if (Initialized == false) {
        Debug.LogError("[NewtonVR] Error: NVRInteractables.Register called before initialization.");
      }

      NVRInteractable interactable;
      ColliderMapping.TryGetValue(collider, out interactable);
      return interactable;
    }
  }
}

﻿using System.Collections.Generic;
using UnityEngine;

namespace NewtonVR {
  public class NVRCollisionSoundProviderUnity : NVRCollisionSoundProvider {
    private static readonly string AudioSourcePrefabPath = "CollisionSoundPrefab";
    private static readonly string CollisionSoundsPath = "CollisionSounds";

    private AudioSource[] AudioPool;
    private GameObject AudioSourcePrefab;

    private Dictionary<NVRCollisionSoundMaterials, List<AudioClip>> Clips;
    private int CurrentPoolIndex;

    public override void Awake() {
      AudioPool = new AudioSource[NVRCollisionSoundController.Instance.SoundPoolSize];

      AudioSourcePrefab = Resources.Load<GameObject>(AudioSourcePrefabPath);

      for (int index = 0; index < AudioPool.Length; index++) {
        AudioPool[index] =
          Instantiate(AudioSourcePrefab).GetComponent<AudioSource>();
        AudioPool[index].transform.parent = transform;
      }

      AudioClip[] clips = Resources.LoadAll<AudioClip>(CollisionSoundsPath);
      Clips = new Dictionary<NVRCollisionSoundMaterials, List<AudioClip>>();
      for (int index = 0; index < clips.Length; index++) {
        string name = clips[index].name;
        int dividerIndex = name.IndexOf("__");
        if (dividerIndex >= 0) {
          name = name.Substring(0, dividerIndex);
        }

        NVRCollisionSoundMaterials? material = NVRCollisionSoundMaterialsList.Parse(name);
        if (material != null) {
          if (Clips.ContainsKey(material.Value) == false || Clips[material.Value] == null) {
            Clips[material.Value] = new List<AudioClip>();
          }
          Clips[material.Value].Add(clips[index]);
        } else {
          Debug.LogWarning(
            "[NewtonVR] CollisionSound: Found clip for material that is not in enumeration (NVRCollisionSoundMaterials): " +
            clips[index].name);
        }
      }
    }

    public override void Play(NVRCollisionSoundMaterials material, Vector3 position,
                              float impactVolume) {
      if (material == NVRCollisionSoundMaterials.none) {
        return;
      }

      if (NVRCollisionSoundController.Instance.PitchModulationEnabled) {
        AudioPool[CurrentPoolIndex].pitch =
          Random.Range(1 - NVRCollisionSoundController.Instance.PitchModulationRange,
            1 + NVRCollisionSoundController.Instance.PitchModulationRange);
      }

      AudioPool[CurrentPoolIndex].transform.position = position;
      AudioPool[CurrentPoolIndex].volume = impactVolume;
      AudioPool[CurrentPoolIndex].clip = GetClip(material);
      AudioPool[CurrentPoolIndex].Play();

      CurrentPoolIndex++;

      if (CurrentPoolIndex >= AudioPool.Length) {
        CurrentPoolIndex = 0;
      }
    }

    private AudioClip GetClip(NVRCollisionSoundMaterials material) {
      if (Clips.ContainsKey(material) == false) {
        material = NVRCollisionSoundMaterials._default;
        Debug.LogError(
          "[NewtonVR] CollisionSound: Trying to play sound for material without a clip. Need a clip at: " +
          CollisionSoundsPath + "/" + material);
      }

      int index = Random.Range(0, Clips[material].Count);

      return Clips[material][index];
    }
  }
}

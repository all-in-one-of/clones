using UnityEngine;

namespace NewtonVR.Example {
  public class NVRExampleGrower : NVRInteractableItem {
    public override void InteractingUpdate(NVRHand hand) {
      base.InteractingUpdate(hand);

      if (hand.Inputs[NVRButtons.Touchpad].PressUp) {
        Vector3 randomPoint = Random.insideUnitSphere;
        randomPoint *= Colliders[0].bounds.extents.x;
        randomPoint += Colliders[0].bounds.center;
        randomPoint += Colliders[0].bounds.extents;

        GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        newCube.transform.parent = transform;
        newCube.transform.localScale = transform.localScale / 2;
        newCube.transform.position = randomPoint;

        UpdateColliders();
      }
    }
  }
}

using UnityEngine;

namespace NewtonVR.Example {
  public class NVRExampleGun : NVRInteractableItem {
    public Vector3 BulletForce = new Vector3(0, 0, 250);
    public GameObject BulletPrefab;

    public Transform FirePoint;

    public override void UseButtonDown() {
      base.UseButtonDown();

      GameObject bullet = Instantiate(BulletPrefab);
      bullet.transform.position = FirePoint.position;
      bullet.transform.forward = FirePoint.forward;

      bullet.GetComponent<Rigidbody>().AddRelativeForce(BulletForce);

      AttachedHand.TriggerHapticPulse(500);
    }
  }
}

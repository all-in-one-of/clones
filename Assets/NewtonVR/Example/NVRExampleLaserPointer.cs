using UnityEngine;

namespace NewtonVR.Example {
  public class NVRExampleLaserPointer : MonoBehaviour {
    public bool ForceLineVisible = true;
    public Color LineColor;
    public float LineWidth = 0.02f;

    public bool OnlyVisibleOnTrigger = true;

    private NVRHand Hand;

    private LineRenderer Line;

    private void Awake() {
      Line = GetComponent<LineRenderer>();
      Hand = GetComponent<NVRHand>();

      if (Line == null) {
        Line = gameObject.AddComponent<LineRenderer>();
      }

      if (Line.sharedMaterial == null) {
        Line.material = new Material(Shader.Find("Unlit/Color"));
        Line.material.SetColor("_Color", LineColor);
        NVRHelpers.LineRendererSetColor(Line, LineColor, LineColor);
      }

      Line.useWorldSpace = true;
    }

    private void LateUpdate() {
      Line.enabled = ForceLineVisible ||
                     (OnlyVisibleOnTrigger && Hand != null && Hand.Inputs[NVRButtons.Trigger].IsPressed);

      if (Line.enabled) {
        Line.material.SetColor("_Color", LineColor);
        NVRHelpers.LineRendererSetColor(Line, LineColor, LineColor);
        NVRHelpers.LineRendererSetWidth(Line, LineWidth, LineWidth);

        RaycastHit hitInfo;
        bool hit = Physics.Raycast(transform.position, transform.forward, out hitInfo, 1000);
        Vector3 endPoint;

        if (hit) {
          endPoint = hitInfo.point;
        } else {
          endPoint = transform.position + (transform.forward * 1000f);
        }

        Line.SetPositions(new[] {transform.position, endPoint});
      }
    }
  }
}

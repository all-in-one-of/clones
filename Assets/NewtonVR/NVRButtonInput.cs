using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NewtonVR {
  public class NVRButtonInputs {
    /// <summary>Is true ONLY on the frame that the button is first pressed down</summary>
    public bool PressDown
    {
      get
      {
        if (PressDownExpired) {
          PressDownCached = input_device.GetPressDown(nvrbutton);
          PressDownExpired = false;
        }
        return PressDownCached;
      }
    }

    private bool PressDownCached;
    private bool PressDownExpired = true;

    /// <summary>Is true ONLY on the frame that the button is released after being pressed down</summary>
    public bool PressUp
    {
      get
      {
        if (PressUpExpired) {
          PressUpCached = input_device.GetPressUp(nvrbutton);
          PressUpExpired = false;
        }
        return PressUpCached;
      }
    }

    private bool PressUpCached;
    private bool PressUpExpired = true;

    /// <summary>Is true WHENEVER the button is pressed down</summary>
    public bool IsPressed
    {
      get
      {
        if (IsPressedExpired) {
          IsPressedCached = input_device.GetPress(nvrbutton);
          IsPressedExpired = false;
        }
        return IsPressedCached;
      }
    }

    private bool IsPressedCached;
    private bool IsPressedExpired = true;

    /// <summary>Is true ONLY on the frame that the button is first touched</summary>
    public bool TouchDown
    {
      get
      {
        if (TouchDownExpired) {
          TouchDownCached = input_device.GetTouchDown(nvrbutton);
          TouchDownExpired = false;
        }
        return TouchDownCached;
      }
    }

    private bool TouchDownCached;
    private bool TouchDownExpired = true;

    /// <summary>Is true ONLY on the frame that the button is released after being touched</summary>
    public bool TouchUp
    {
      get
      {
        if (TouchUpExpired) {
          TouchUpCached = input_device.GetTouchUp(nvrbutton);
          TouchUpExpired = false;
        }
        return TouchUpCached;
      }
    }

    private bool TouchUpCached;
    private bool TouchUpExpired = true;

    /// <summary>Is true WHENEVER the button is being touched</summary>
    public bool IsTouched
    {
      get
      {
        if (IsTouchedExpired) {
          IsTouchedCached = input_device.GetTouch(nvrbutton);
          IsTouchedExpired = false;
        }
        return IsTouchedCached;
      }
    }

    private bool IsTouchedCached;
    private bool IsTouchedExpired = true;

    /// <summary>Is true ONLY on the frame that the button is first near touched</summary>
    public bool NearTouchDown
    {
      get
      {
        if (NearTouchDownExpired) {
          NearTouchDownCached = input_device.GetNearTouchDown(nvrbutton);
          NearTouchDownExpired = false;
        }
        return NearTouchDownCached;
      }
    }

    private bool NearTouchDownCached;
    private bool NearTouchDownExpired = true;

    /// <summary>Is true ONLY on the frame that the button is released after being near touched</summary>
    public bool NearTouchUp
    {
      get
      {
        if (NearTouchUpExpired) {
          NearTouchUpCached = input_device.GetNearTouchUp(nvrbutton);
          NearTouchUpExpired = false;
        }
        return NearTouchUpCached;
      }
    }

    private bool NearTouchUpCached;
    private bool NearTouchUpExpired = true;

    /// <summary>Is true WHENEVER the button is near being touched</summary>
    public bool IsNearTouched
    {
      get
      {
        if (IsNearTouchedExpired) {
          IsNearTouchedCached = input_device.GetNearTouch(nvrbutton);
          IsNearTouchedExpired = false;
        }
        return IsNearTouchedCached;
      }
    }

    private bool IsNearTouchedCached;
    private bool IsNearTouchedExpired = true;

    /// <summary>x,y axis generally for the touchpad. trigger uses x</summary>
    public Vector2 Axis
    {
      get
      {
        if (AxisExpired) {
          AxisCached = input_device.GetAxis2D(nvrbutton);
          AxisExpired = false;
        }
        return AxisCached;
      }
    }

    private Vector2 AxisCached;
    private bool AxisExpired = true;

    /// <summary>x axis from Axis</summary>
    public float SingleAxis
    {
      get
      {
        if (SingleAxisExpired) {
          SingleAxisCached = input_device.GetAxis1D(nvrbutton);
          SingleAxisExpired = false;
        }
        return SingleAxisCached;
      }
    }

    private float SingleAxisCached;
    private bool SingleAxisExpired = true;

    private readonly NVRInputDevice input_device;
    private readonly NVRButtons nvrbutton;

    public NVRButtonInputs(NVRInputDevice device, NVRButtons button) {
      nvrbutton = button;
      input_device = device;
    }

    /// <summary>
    /// Reset the cached values for a new frame.
    /// </summary>
    /// <param name="inputDevice">NVRInputDevice</param>
    /// <param name="button">NVRButtons</param>
    public void FrameReset() {
      PressDownExpired = true;
      PressUpExpired = true;
      IsPressedExpired = true;
      TouchDownExpired = true;
      TouchUpExpired = true;
      IsTouchedExpired = true;
      NearTouchDownExpired = true;
      NearTouchUpExpired = true;
      IsNearTouchedExpired = true;
      AxisExpired = true;
      SingleAxisExpired = true;
    }
  }
}

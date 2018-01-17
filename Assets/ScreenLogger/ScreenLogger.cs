using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AClockworkBerry {
  public class ScreenLogger : MonoBehaviour {
    private class LogMessage {
      public readonly string Message;
      public readonly LogType Type;

      public LogMessage(string msg, LogType type) {
        Message = msg;
        Type = type;
      }
    }

    public enum LogAnchor {
      TopLeft,
      TopRight,
      BottomLeft,
      BottomRight
    }

    public static bool IsPersistent = true;

    private static ScreenLogger instance;
    private static bool instantiated;

    private static Queue<LogMessage> queue = new Queue<LogMessage>();

    public LogAnchor AnchorPosition = LogAnchor.BottomLeft;
    public Color BackgroundColor = Color.black;

    [Range(0f, 01f)] public float BackgroundOpacity = 0.5f;
    public Canvas canvas = null;
    public Color ErrorColor = new Color(1, 0.5f, 0.5f);

    public int FontSize = 14;

    [Tooltip("Height of the log area as a percentage of the screen height")] [Range(0.3f, 1.0f)] public float Height =
      0.5f;

    public bool LogErrors = true;

    public bool LogMessages = true;
    public bool LogWarnings = true;

    public int Margin = 20;

    public Color MessageColor = Color.white;
    public Text sample_text = null;
    public bool ShowInEditor = true;

    public bool ShowLog = true;
    public bool StackTraceErrors = true;

    public bool StackTraceMessages = false;
    public bool StackTraceWarnings = false;

    public int TotalRows = 50;
    public Color WarningColor = Color.yellow;

    [Tooltip("Width of the log area as a percentage of the screen width")] [Range(0.3f, 1.0f)] public float Width = 0.5f;

    private bool destroying;
    private readonly List<Text> line_ui = new List<Text>();
    private readonly int padding = 5;

    private GUIStyle styleContainer, styleText;

    public static ScreenLogger Instance {
      get {
        if (instantiated) return instance;

        instance = FindObjectOfType(typeof(ScreenLogger)) as ScreenLogger;

        // Object not found, we create a new one
        if (instance == null) {
          // Try to load the default prefab
          try {
            instance = Instantiate(Resources.Load("ScreenLoggerPrefab", typeof(ScreenLogger))) as ScreenLogger;
          } catch (Exception e) {
            Debug.Log("Failed to load default Screen Logger prefab...");
            instance = new GameObject("ScreenLogger", typeof(ScreenLogger)).GetComponent<ScreenLogger>();
          }

          // Problem during the creation, this should not happen
          if (instance == null) {
            Debug.LogError("Problem during the creation of ScreenLogger");
          } else instantiated = true;
        } else {
          instantiated = true;
        }

        return instance;
      }
    }

    public void Awake() {
      ScreenLogger[] obj = FindObjectsOfType<ScreenLogger>();

      if (obj.Length > 1) {
        Debug.Log("Destroying ScreenLogger, already exists...");

        destroying = true;

        Destroy(gameObject);
        return;
      }

      InitStyles();

      if (IsPersistent) DontDestroyOnLoad(this);
    }

    private void InitStyles() {
      Texture2D back = new Texture2D(1, 1);
      BackgroundColor.a = BackgroundOpacity;
      back.SetPixel(0, 0, BackgroundColor);
      back.Apply();

      styleContainer = new GUIStyle();
      styleContainer.normal.background = back;
      styleContainer.wordWrap = false;
      styleContainer.padding = new RectOffset(padding, padding, padding, padding);

      styleText = new GUIStyle();
      styleText.fontSize = FontSize;
    }

    private void OnEnable() {
      if (!ShowInEditor && Application.isEditor) return;

      queue = new Queue<LogMessage>();

#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            Application.RegisterLogCallback(HandleLog);
#else
      Application.logMessageReceived += HandleLog;
#endif
    }

    private void OnDisable() {
      // If destroyed because already exists, don't need to de-register callback
      if (destroying) return;

#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            Application.RegisterLogCallback(null);
#else
      Application.logMessageReceived -= HandleLog;
#endif
    }

    private void Update() {
      if (!ShowInEditor && Application.isEditor) return;

      float InnerHeight = (Screen.height - 2 * Margin) * Height - 2 * padding;

      // Remove overflowing rows
      while (queue.Count > TotalRows) queue.Dequeue();

      RenderGUI();
    }

    private void Start() {
      RectTransform canvas_rt = canvas.GetComponent<RectTransform>();

      for (int i = 0; i < TotalRows; i++) {
        GameObject textbox = Instantiate(sample_text.gameObject);
        textbox.SetActive(true);
        RectTransform rt = textbox.GetComponent<RectTransform>();
        rt.SetParent(canvas_rt);
        rt.localPosition = new Vector3(0, i * (sample_text.fontSize + 5), 0);
        rt.localScale = new Vector3(1, 1, 1);
        rt.localRotation = Quaternion.identity;
        Text t = textbox.GetComponent<Text>();
        t.text = "test!";

        line_ui.Add(t);
      }
    }

    private void RenderGUI() {
      if (!ShowLog) return;
      if (!ShowInEditor && Application.isEditor) return;

      int line_number = 0;
      foreach (LogMessage m in queue.Reverse()) {
        Text t = line_ui[line_number];
        switch (m.Type) {
          case LogType.Warning:
            t.color = WarningColor;
            break;

          case LogType.Log:
            t.color = MessageColor;
            break;

          case LogType.Assert:
          case LogType.Exception:
          case LogType.Error:
            t.color = ErrorColor;
            break;

          default:
            t.color = MessageColor;
            break;
        }

        t.text = m.Message;
        line_number++;
      }
    }

    private void HandleLog(string message, string stackTrace, LogType type) {
      if (type == LogType.Assert && !LogErrors) return;
      if (type == LogType.Error && !LogErrors) return;
      if (type == LogType.Exception && !LogErrors) return;
      if (type == LogType.Log && !LogMessages) return;
      if (type == LogType.Warning && !LogWarnings) return;

      string[] lines = message.Split('\n');

      foreach (string l in lines) queue.Enqueue(new LogMessage(l, type));

      if (type == LogType.Assert && !StackTraceErrors) return;
      if (type == LogType.Error && !StackTraceErrors) return;
      if (type == LogType.Exception && !StackTraceErrors) return;
      if (type == LogType.Log && !StackTraceMessages) return;
      if (type == LogType.Warning && !StackTraceWarnings) return;

      string[] trace = stackTrace.Split('\n');

      foreach (string t in trace) if (t.Length != 0) queue.Enqueue(new LogMessage("  " + t, type));
    }

    public void InspectorGUIUpdated() {
      InitStyles();
    }
  }
}

/*
The MIT License

Copyright © 2016 Screen Logger - Giuseppe Portelli <giuseppe@aclockworkberry.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

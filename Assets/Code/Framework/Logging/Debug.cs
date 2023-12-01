using System;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;
using UDebug = UnityEngine.Debug;

namespace Framework.Logging
{
  public class Debug
  {
    private readonly Debug _parent;
    private readonly string _name;
    private readonly string _fullName;

    public Debug(string name, Debug parent = null)
    {
      _name = name;
      _parent = parent;
      _fullName = $"{FullName}: ";
    }

    public bool Mute { get; set; }

    // выключать после тестов! ( for tests only! )
    public bool ForceLog { get; set; }

    public bool IsMute
      => !ForceLog && (Mute || _parent?.IsMute == true);

    private string FullName
      => (_parent == null ? string.Empty : _parent.FullName + '.') + _name;

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void Assert(bool condition, object message = null)
    {
      UDebug.Assert(condition, message);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0, bool depthTest = true)
    {
      UDebug.DrawLine(start, end, color, duration, depthTest);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void DrawLine(Vector3 start, Vector3 end)
    {
      UDebug.DrawLine(start, end);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration = 0, bool depthTest = true)
    {
      UDebug.DrawRay(start, dir, color, duration, depthTest);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void DrawRay(Vector3 start, Vector3 dir)
    {
      UDebug.DrawRay(start, dir);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void Log(object message, Object obj = null)
    {
      UDebug.Log(message, obj);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void LogFormat(string format, params object[] args)
    {
      UDebug.LogFormat(format, args);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void LogAssertion(object message, Object obj = null)
    {
      UDebug.LogAssertion(message, obj);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void LogWarning(object message, Object obj = null)
    {
      UDebug.LogWarning(message, obj);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void LogWarningFormat(string format, params object[] args)
    {
      UDebug.LogWarningFormat(format, args);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void LogError(object message, Object obj = null)
    {
      UDebug.LogError(message, obj);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void LogErrorFormat(string format, params object[] args)
    {
      UDebug.LogErrorFormat(format, args);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public static void LogException(Exception exception, Object obj = null)
    {
      UDebug.LogException(exception, obj);
    }

    public static void Break()
    {
      UDebug.Break();
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public void Info(string msg, Object obj = null)
    {
      if (!IsMute)
      {
        UDebug.Log(_fullName + msg, obj);
      }
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public void Warn(string msg, Object obj = null)
    {
      if (!IsMute)
      {
        UDebug.LogWarning(_fullName + msg, obj);
      }
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public void Err(string msg, Object obj = null)
    {
      UDebug.LogError("LogError:" + _fullName + msg, obj);
    }

    [Conditional("DEBUG_ENABLE_LOG")]
    public void Ex(Exception ex, Object obj = null)
    {
      UDebug.LogError(_fullName + ex, obj);
    }
  }
}
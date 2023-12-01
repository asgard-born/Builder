using System;
using Framework.Logging;

namespace Framework.Async
{
  internal static class DelegateExtensions
  {
    public static void TryInvoke(this Action action, Debug log = null)
    {
      if (action == null)
      {
        return;
      }
      try
      {
        action();
      }
      catch (Exception exception)
      {
        if (log == null)
        {
          Debug.LogException(exception);
        }
        else
        {
          log.Ex(exception);
        }
      }
    }
    
    public static void TryInvoke<T>(this Action<T> action, T arg, Debug log = null)
    {
      if (action == null)
      {
        return;
      }
      try
      {
        action(arg);
      }
      catch (Exception exception)
      {
        if (log == null)
        {
          Debug.LogException(exception);
        }
        else
        {
          log.Ex(exception);
        }
      }
    }
    
    public static void TryInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2, Debug log = null)
    {
      if (action == null)
      {
        return;
      }
      try
      {
        action(arg1, arg2);
      }
      catch (Exception exception)
      {
        if (log == null)
        {
          Debug.LogException(exception);
        }
        else
        {
          log.Ex(exception);
        }
      }
    }
    
    public static void TryInvoke<T1, T2, T3>(this Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3, Debug log = null)
    {
      if (action == null)
      {
        return;
      }
      try
      {
        action(arg1, arg2, arg3);
      }
      catch (Exception exception)
      {
        if (log == null)
        {
          Debug.LogException(exception);
        }
        else
        {
          log.Ex(exception);
        }
      }
    }
    
    public static T TryInvoke<T>(this Func<T> func, Debug log = null)
    {
      if (func == null)
      {
        return default;
      }
      try
      {
        return func();
      }
      catch (Exception exception)
      {
        if (log == null)
        {
          Debug.LogException(exception);
        }
        else
        {
          log.Ex(exception);
        }
        return default;
      }
    }
    
    public static Tret TryInvoke<T, Tret>(this Func<T, Tret> func, T arg, Debug log = null)
    {
      if (func == null)
      {
        return default;
      }
      try
      {
        return func(arg);
      }
      catch (Exception exception)
      {
        if (log == null)
        {
          Debug.LogException(exception);
        }
        else
        {
          log.Ex(exception);
        }
        return default;
      }
    }
    
    public static Tret TryInvoke<T1, T2, Tret>(this Func<T1, T2, Tret> func, T1 arg1, T2 arg2, Debug log = null)
    {
      if (func == null)
      {
        return default;
      }
      try
      {
        return func(arg1, arg2);
      }
      catch (Exception exception)
      {
        if (log == null)
        {
          Debug.LogException(exception);
        }
        else
        {
          log.Ex(exception);
        }
        return default;
      }
    }
    
    public static Tret TryInvoke<T1, T2, T3, Tret>(this Func<T1, T2, T3, Tret> func, T1 arg1, T2 arg2, T3 arg3,
      Debug log = null)
    {
      if (func == null)
      {
        return default;
      }
      try
      {
        return func(arg1, arg2, arg3);
      }
      catch (Exception exception)
      {
        if (log == null)
        {
          Debug.LogException(exception);
        }
        else
        {
          log.Ex(exception);
        }
        return default;
      }
    }
  }
}
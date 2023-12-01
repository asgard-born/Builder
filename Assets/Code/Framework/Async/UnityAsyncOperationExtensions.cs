using System;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Debug = Framework.Logging.Debug;
using Object = UnityEngine.Object;

namespace Framework.Async
{
  internal static class UnityAsyncOperationExtensions
  {
    public static IAwaiter GetAwaiter(this AsyncOperation operation)
    {
      return new UnityAsyncOperationAwaiter(operation);
    }

    public static UnityAsyncOperationAwaiter<AssetBundle> GetAwaiter(this AssetBundleCreateRequest req)
    {
      return new UnityAsyncOperationAwaiter<AssetBundle>(req, op => ((AssetBundleCreateRequest)op).assetBundle);
    }

    public static UnityAsyncOperationAwaiter<Object> GetAwaiter(this AssetBundleRequest req)
    {
      return new UnityAsyncOperationAwaiter<Object>(req, op => ((AssetBundleRequest)op).asset);
    }

    public static UnityAsyncOperationAwaiter<Object> GetAwaiter(this ResourceRequest req)
    {
      return new UnityAsyncOperationAwaiter<Object>(req, op => ((ResourceRequest) op).asset);
    }

    public static UnityAsyncOperationDisposableAwaiter AsDisposable(this IAwaiter awaiter)
    {
      return new UnityAsyncOperationDisposableAwaiter(awaiter);
    }
    public static UnityAsyncOperationDisposableAwaiter<T> AsDisposable<T>(this IAwaiter<T> awaiter)
    {
      return new UnityAsyncOperationDisposableAwaiter<T>(awaiter);
    }
    
    public static TaskDisposableAwaiter AsDisposable(this Task task)
    {
      return new TaskDisposableAwaiter(task.GetAwaiter());
    }
    
    public static TaskDisposableAwaiter<T> AsDisposable<T>(this Task<T> task)
    {
      return new TaskDisposableAwaiter<T>(task.GetAwaiter());
    }
    
    public static void TryInvokeOnMainThreadOrSchedule(this Action action)
    {
      if (action == null)
      {
        return;
      }
      if (!MainThreadDispatcher.IsInitialized) return;
      if (MainThreadDispatcher.IsInMainThread)
      {
        action();
      }
      else
      {
        MainThreadDispatcher.Post(_ => action(), null);
      }
    }
    
    public static void TryInvokeOnMainThreadOrSchedule<T>(this Action<T> action, T arg)
    {
      if (action == null)
      {
        return;
      }
      if (!MainThreadDispatcher.IsInitialized) return;
      if (MainThreadDispatcher.IsInMainThread)
      {
        action(arg);
      }
      else
      {
        MainThreadDispatcher.Post(obj => action((T)obj), arg);
      }
    }
    
    public static void TryInvokeOnMainThreadOrSchedule<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
    {
      if (action == null)
      {
        return;
      }
      if (!MainThreadDispatcher.IsInitialized) return;
      if (MainThreadDispatcher.IsInMainThread)
      {
        action(arg1, arg2);
      }
      else
      {
        void PostAction(object obj)
        {
          (T1 item1, T2 item2) = (Tuple<T1, T2>) obj;
          action(item1, item2);
        }
        MainThreadDispatcher.Post(PostAction, new Tuple<T1, T2>(arg1, arg2));
      }
    }

    public static void ScheduleOnMainThread(this Action action, Debug log = null)
    {
      if (!MainThreadDispatcher.IsInitialized) return;
      MainThreadDispatcher.Post(_ => action.TryInvoke(log), null);
    }
  }
}
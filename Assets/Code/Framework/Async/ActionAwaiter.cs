using System;

namespace Framework.Async
{
  internal class ActionAwaiter : IAwaiter
  {
    private Action _onComplete;
    
    public bool IsCompleted { get; private set; }

    public void OnCompleted(Action continuation)
    {
      lock (this)
      {
        if (!IsCompleted)
        {
          _onComplete += continuation;
          return;
        }
      }
      continuation.TryInvokeOnMainThreadOrSchedule();
    }

    public void GetResult()
    {
      if (!IsCompleted)
      {
        throw new InvalidOperationException("operation isn't finished yet");
      }
    }
    
    public IAwaiter GetAwaiter()
      => this;

    public void Continue()
    {
      Action callback;
      lock (this)
      {
        if (IsCompleted)
        {
          return;
        }
        IsCompleted = true;
        callback = _onComplete;
        _onComplete = null;
      }
      callback.TryInvokeOnMainThreadOrSchedule();
    }
  }
  
  internal class ActionAwaiter<T> : IAwaiter<T>
  {
    private T _arg;
    private Action _onComplete;
    
    public bool IsCompleted { get; private set; }

    public void OnCompleted(Action continuation)
    {
      lock (this)
      {
        if (!IsCompleted)
        {
          _onComplete += continuation;
          return;
        }
      }
      continuation.TryInvokeOnMainThreadOrSchedule();
    }

    public T GetResult()
    {
      if (!IsCompleted)
      {
        throw new InvalidOperationException("operation isn't finished yet");
      }
      return _arg;
    }
    
    public IAwaiter<T> GetAwaiter()
      => this;
    
    IAwaiter IAwaiter.GetAwaiter()
      => GetAwaiter();

    void IAwaiter.GetResult()
      => GetResult();

    public void Continue(T arg)
    {
      Action callback;
      lock (this)
      {
        if (IsCompleted)
        {
          return;
        }
        IsCompleted = true;
        _arg = arg;
        callback = _onComplete;
        _onComplete = null;
      }
      callback.TryInvokeOnMainThreadOrSchedule();
    }
  }
}
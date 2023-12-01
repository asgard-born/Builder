using System;
using System.Runtime.CompilerServices;

namespace Framework.Async
{
  internal class TaskDisposableAwaiter : IDisposableAwaiter
  {
    private TaskAwaiter _awaiter;
    private Action _onComplete;
    private bool _disposed;

    public bool IsCompleted
      => _awaiter.IsCompleted;

    public TaskDisposableAwaiter(TaskAwaiter awaiter)
    {
      _awaiter = awaiter;
      awaiter.OnCompleted(OnAwaiterCompleted);
    }

    public void OnCompleted(Action continuation)
    {
      lock (this)
      {
        if (_disposed)
        {
          return;
        }
        if (!_awaiter.IsCompleted)
        {
          _onComplete += continuation;
          return;
        }
      }
      continuation.TryInvokeOnMainThreadOrSchedule();
    }

    public void GetResult()
      => _awaiter.GetResult();
    
    public IAwaiter GetAwaiter()
      => this;

    public void Dispose()
    {
      lock (this)
      {
        if (_disposed)
        {
          return;
        }
        _disposed = true;
        _onComplete = null;
      }
    }

    private void OnAwaiterCompleted()
    {
      Action callback;
      lock (this)
      {
        if (_disposed || _onComplete == null)
        {
          return;
        }
        callback = _onComplete;
        _onComplete = null;
      }
      callback.TryInvokeOnMainThreadOrSchedule();
    }
  }
  
  internal class TaskDisposableAwaiter<T> : IDisposableAwaiter<T>
  {
    private TaskAwaiter<T> _awaiter;
    private Action _onComplete;
    private bool _disposed;

    public bool IsCompleted
      => _awaiter.IsCompleted;

    public TaskDisposableAwaiter(TaskAwaiter<T> awaiter)
    {
      _awaiter = awaiter;
      awaiter.OnCompleted(OnAwaiterCompleted);
    }

    public void OnCompleted(Action continuation)
    {
      lock (this)
      {
        if (_disposed)
        {
          return;
        }
        if (!_awaiter.IsCompleted)
        {
          _onComplete += continuation;
          return;
        }
      }
      continuation.TryInvokeOnMainThreadOrSchedule();
    }

    public T GetResult()
      => _awaiter.GetResult();

    IAwaiter IAwaiter.GetAwaiter()
      => GetAwaiter();

    void IAwaiter.GetResult()
      => GetResult();

    public IAwaiter<T> GetAwaiter()
      => this;

    public void Dispose()
    {
      lock (this)
      {
        if (_disposed)
        {
          return;
        }
        _disposed = true;
        _onComplete = null;
      }
    }

    private void OnAwaiterCompleted()
    {
      Action callback;
      lock (this)
      {
        if (_disposed || _onComplete == null)
        {
          return;
        }
        callback = _onComplete;
        _onComplete = null;
      }
      callback.TryInvokeOnMainThreadOrSchedule();
    }
  }
}
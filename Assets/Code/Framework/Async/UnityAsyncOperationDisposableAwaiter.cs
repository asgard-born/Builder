using System;

namespace Framework.Async
{
  internal class UnityAsyncOperationDisposableAwaiter : IDisposableAwaiter
  {
    private readonly IAwaiter _awaiter;

    private Action _onComplete;
    private bool _disposed;

    public bool IsCompleted
      => _awaiter.IsCompleted;

    public UnityAsyncOperationDisposableAwaiter(IAwaiter awaiter)
    {
      _awaiter = awaiter ?? throw new ArgumentNullException(nameof(awaiter));
      _awaiter.OnCompleted(OnAwaiterCompleted);
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
  
  internal class UnityAsyncOperationDisposableAwaiter<T> : IDisposableAwaiter<T>
  {
    private readonly IAwaiter<T> _awaiter;

    private Action _onComplete;
    private bool _disposed;

    public bool IsCompleted
      => _awaiter.IsCompleted;

    public UnityAsyncOperationDisposableAwaiter(IAwaiter<T> awaiter)
    {
      _awaiter = awaiter ?? throw new ArgumentNullException(nameof(awaiter));
      _awaiter.OnCompleted(OnAwaiterCompleted);
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
    
    public IAwaiter<T> GetAwaiter()
      => this;
    
    IAwaiter IAwaiter.GetAwaiter()
      => GetAwaiter();

    void IAwaiter.GetResult()
      => GetResult();

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
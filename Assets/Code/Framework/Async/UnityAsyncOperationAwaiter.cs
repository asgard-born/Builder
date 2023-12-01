using System;
using UnityEngine;

namespace Framework.Async
{
  internal class UnityAsyncOperationAwaiter : IAwaiter
  {
    private readonly AsyncOperation _operation;
    private Action _onComplete;
    
    public bool IsCompleted
      => _operation.isDone;

    public UnityAsyncOperationAwaiter(AsyncOperation operation)
    {
      _operation = operation ?? throw new ArgumentNullException(nameof(operation));
      operation.completed += OnCompleted;
      if (operation.isDone)
      {
        OnCompleted(operation);
      }
    }

    public void OnCompleted(Action continuation)
    {
      lock (this)
      {
        if (!_operation.isDone)
        {
          _onComplete += continuation;
          return;
        }
      }
      continuation.TryInvokeOnMainThreadOrSchedule();
    }

    public void GetResult()
    {
      if (!_operation.isDone)
      {
        throw new InvalidOperationException("operation isn't finished yet");
      }
    }
    
    public IAwaiter GetAwaiter()
      => this;
    
    private void OnCompleted(AsyncOperation operation)
    {
      Action callback;
      lock (this)
      {
        if (_onComplete == null)
        {
          return;
        }
        callback = _onComplete;
        operation.completed -= OnCompleted;
        _onComplete = null;
      }
      callback.TryInvokeOnMainThreadOrSchedule();
    }
  }
  
  internal class UnityAsyncOperationAwaiter<T> : IAwaiter<T>
  {
    private readonly AsyncOperation _operation;
    private readonly Func<AsyncOperation, T> _getResult;
    private Action _onComplete;
    
    public bool IsCompleted
      => _operation.isDone;

    public UnityAsyncOperationAwaiter(AsyncOperation operation, Func<AsyncOperation, T> getResult)
    {
      _operation = operation ?? throw new ArgumentNullException(nameof(operation));
      _getResult = getResult ?? throw new ArgumentNullException(nameof(getResult));
      operation.completed += OnCompleted;
      if (operation.isDone)
      {
        OnCompleted(operation);
      }
    }

    public void OnCompleted(Action continuation)
    {
      lock (this)
      {
        if (!_operation.isDone)
        {
          _onComplete += continuation;
          return;
        }
      }
      continuation.TryInvokeOnMainThreadOrSchedule();
    }

    public T GetResult()
    {
      if (!_operation.isDone)
      {
        throw new InvalidOperationException("operation isn't finished yet");
      }
      return _getResult(_operation);
    }
    
    public IAwaiter<T> GetAwaiter()
      => this;
    
    IAwaiter IAwaiter.GetAwaiter()
      => GetAwaiter();

    void IAwaiter.GetResult()
      => GetResult();

    private void OnCompleted(AsyncOperation operation)
    {
      Action callback;
      lock (this)
      {
        if (_onComplete == null)
        {
          return;
        }
        callback = _onComplete;
        operation.completed -= OnCompleted;
        _onComplete = null;
      }
      callback.TryInvokeOnMainThreadOrSchedule();
    }
  }
}
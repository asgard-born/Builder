using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Framework.Addressables;
using Framework.Async;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Debug = Framework.Logging.Debug;
using Object = UnityEngine.Object;

namespace Framework
{
    public abstract class BaseDisposable : IDisposable
    {
        private bool _isDisposed;
        private List<IDisposable> _mainThreadDisposables;
        private List<Object> _unityObjects;
        private HashSet<IDisposableAwaiter> _operations;
        private List<Task> _taskOperations;

        protected bool IsDisposed
            => _isDisposed;
        
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;

            if (_operations != null)
            {
                foreach (IDisposableAwaiter operation in _operations)
                {
                    operation.Dispose();
                }
                _operations.Clear();
                _operations = null;
            }

            if (_mainThreadDisposables != null)
            {
                List<IDisposable> mainThreadDisposables = _mainThreadDisposables;
                for (int i = mainThreadDisposables.Count - 1; i >= 0; i--)
                {
                    mainThreadDisposables[i]?.Dispose();
                }
                mainThreadDisposables.Clear();
            }

            try
            {
                OnDispose();
            }
            catch (Exception e)
            {
                Debug.Log($"This exception can be ignored. Disposable of {GetType().Name}: {e}");
            }

            if (_unityObjects != null)
            {
                foreach (Object obj in _unityObjects)
                {
                    if (obj)
                        Object.Destroy(obj);
                }
            }
        }

        protected virtual void OnDispose()
        {
        }

        public TDisposable AddUnsafe<TDisposable>(TDisposable disposable) where TDisposable : IDisposable
        {
            if (_isDisposed)
            {
                Debug.Log("disposed");
                return default;
            }
            if (disposable == null)
            {
                return default;
            }

            if (_mainThreadDisposables == null)
            {
                _mainThreadDisposables = new List<IDisposable>(1);
            }
            _mainThreadDisposables.Add(disposable);
            return disposable;
        }

        protected IAwaiter KeepOperation(IAwaiter awaiter)
        {
            if (_isDisposed)
            {
                Debug.Log("disposed");
                return default;
            }
            if (awaiter == null)
            {
                Debug.Log("can't keep null operation");
                return default;
            }
            if (awaiter.IsCompleted)
            {
                return awaiter;
            }
            IDisposableAwaiter disposableAwaiter = awaiter.AsDisposable();
            TrackOperation(disposableAwaiter);
            return disposableAwaiter;
        }
        protected IAwaiter<T> KeepOperation<T>(IAwaiter<T> awaiter)
        {
            if (_isDisposed)
            {
                Debug.Log("disposed");
                return default;
            }
            if (awaiter == null)
            {
                Debug.Log("can't keep null operation");
                return default;
            }
            if (awaiter.IsCompleted)
            {
                return awaiter;
            }
            IDisposableAwaiter<T> disposableAwaiter = awaiter.AsDisposable();
            TrackOperation(disposableAwaiter);
            return disposableAwaiter;
        }

        protected IAwaiter KeepOperation(Task task)
        {
            if (_isDisposed)
            {
                Debug.Log("disposed");
                return default;
            }
            if (task == null)
            {
                Debug.Log("can't keep null operation");
                return default;
            }
            IDisposableAwaiter disposableAwaiter = task.AsDisposable();
            if (!disposableAwaiter.IsCompleted)
            {
                TrackOperation(disposableAwaiter);
            }
            return disposableAwaiter;
        }

        protected IAwaiter<T> KeepOperation<T>(Task<T> task)
        {
            if (_isDisposed)
            {
                Debug.Log("disposed");
                return default;
            }
            if (task == null)
            {
                Debug.Log("can't keep null operation");
                return default;
            }
            IDisposableAwaiter<T> disposableAwaiter = task.AsDisposable();
            if (!disposableAwaiter.IsCompleted)
            {
                TrackOperation(disposableAwaiter);
            }
            return disposableAwaiter;
        }

        protected void RunAsync(Func<Task> fnc, Action<Exception> onError)
        {
            AsyncOp();
            async Task AsyncOp()
            {
                try
                {
                    await fnc.Invoke();
                }
                catch (Exception e)
                {
                    onError(e);
                    throw;
                }
            }
        }

        protected void RunAsyncAndWait(Func<Task> fnc)
        {
            Task.Run(async () => await fnc())
                .Wait();
        }

        protected Object AttachComponent(Object obj)
        {
            if (_isDisposed)
            {
                Debug.Log("disposed");
                return default;
            }
            if (!obj)
            {
                Debug.Log("can't add null object");
                return default;
            }
            if (_unityObjects == null)
            {
                _unityObjects = new List<Object>(1);
            }
            _unityObjects.Add(obj);
            return obj;
        }

        protected async Task<(T, AddressableRetain)> LoadResource<T>(AssetReference reference) where T : class
        {
            if (reference == null)
            {
                Debug.LogError("reference can't be null");
                return default;
            }
            
            (T resource, AddressableRetain retain) = await reference.TryLoadAsync<T>();
            
            if (_isDisposed)
            {
                Debug.LogWarning($"end load addressable resource on disposed {GetType().Name}");
            }

            string resName = reference.SubObjectName;
            if (resource == null)
            {
                Debug.LogError($"can't load addressable resource {resName}");
                return default;
            }

            if (IsDisposed)
            {
                retain.Dispose();
                return default;
            }

            return (resource, retain);
        }

        protected async Task<T> LoadAndTrackResource<T>(AssetReference reference) where T : class
        {
            (T instance, AddressableRetain addressableRetain) = await LoadResource<T>(reference);
            AddUnsafe(addressableRetain);
            return instance;
        }

        protected async Task<(T, AddressableRetain)> LoadAndInstantiatePrefab<T>(AssetReference reference, Transform parent) where T : class
        {
            (GameObject prefab, AddressableRetain retain) res = await LoadPrefab(reference);
            if (res.prefab == null)
            {
                Debug.LogError("prefab is null");
                return default;
            }

            GameObject instance = Object.Instantiate(res.prefab, Vector3.zero, Quaternion.Euler(0, 0, 0), parent);
            if (instance == null)
            {
                Debug.LogError("can't instantiate prefab");
                return default;
            }

            instance.transform.localPosition = Vector3.zero;
            T comp = instance.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogError($"can't get component for type {typeof(T)}");
                return default;
            }
            
            return (comp, res.retain);
        }

        protected async Task<(GameObject, AddressableRetain)> LoadPrefab(AssetReference reference)
        {
            if (reference == null)
            {
                Debug.LogError("reference can't be null");
                return default;
            }
            
            (GameObject gameObject, AddressableRetain retain) = await reference.TryLoadGameObjAsync();
            
            if (_isDisposed)
            {
                Debug.LogWarning($"end load addressable resource on disposed {GetType().Name}");
            }

            string resName = reference.SubObjectName;
            if (!gameObject)
            {
                Debug.LogError($"can't load addressable resource {resName}");
                return default;
            }

            if (IsDisposed)
            {
                retain.Dispose();
                return default;
            }

            return (gameObject, retain);
        }

        protected async Task WaitUntil(Func<bool> cbUntil)
        {
            while (!cbUntil())
                await Task.Yield();
        }

        protected async Task<T> LoadAndTrackAssetByAddress<T>(string address)
        {
            (T asset, AddressableRetain retain) = await address.TryLoadAsync<T>();
            AddUnsafe(retain);

            return asset;
        }

        protected void BeginTaskOperations()
        {
            _taskOperations?.Clear();
            _taskOperations = new List<Task>();
        }

        protected T AddTaskOperation<T>(T task) where T: Task
        {
            _taskOperations.Add(task);
            return task;
        }

        protected List<Task> GetAllTaskOperations()
        {
            return _taskOperations;
        }

        protected void EndTaskOperations()
        {
            _taskOperations?.Clear();
            _taskOperations = null;
        }

        private void TrackOperation(IDisposableAwaiter awaiter)
        {
            _operations = _operations ?? new HashSet<IDisposableAwaiter>();
            _operations.Add(awaiter);
            awaiter.OnCompleted(() => _operations?.Remove(awaiter));
        }
    }
}
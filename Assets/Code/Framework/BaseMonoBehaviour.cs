using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Framework.Async;
using UnityEngine;
using Debug = Framework.Logging.Debug;

namespace Framework
{
    public class BaseMonoBehaviour : MonoBehaviour
    {
        private HashSet<IDisposableAwaiter> _operations;

        protected virtual void Awake()
        {
        }

        protected virtual void OnDestroy()
        {
            if (_operations != null)
            {
                foreach (IDisposableAwaiter operation in _operations)
                {
                    operation.Dispose();
                }

                _operations.Clear();
                _operations = null;
            }

            FieldInfo[] allFields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in allFields)
            {
                Type fieldType = field.FieldType;

                if (typeof(IList).IsAssignableFrom(fieldType))
                {
                    if (field.GetValue(this) is IList list)
                    {
                        list.Clear();
                    }
                }

                if (typeof(IDictionary).IsAssignableFrom(fieldType))
                {
                    if (field.GetValue(this) is IDictionary dictionary)
                    {
                        dictionary.Clear();
                    }
                }

                if (!fieldType.IsPrimitive)
                {
                    field.SetValue(this, null);
                }
            }
        }

        protected IAwaiter KeepOperation(IAwaiter awaiter)
        {
            if (awaiter == null)
            {
                Debug.LogError("can't keep null awaiter", this);

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
            if (awaiter == null)
            {
                Debug.LogError("can't keep null awaiter", this);

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
            if (task == null)
            {
                Debug.LogError("can't keep null task", this);

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
            if (task == null)
            {
                Debug.LogError("can't keep null task", this);

                return default;
            }

            IDisposableAwaiter<T> disposableAwaiter = task.AsDisposable();

            if (!disposableAwaiter.IsCompleted)
            {
                TrackOperation(disposableAwaiter);
            }

            return disposableAwaiter;
        }

        private void TrackOperation(IDisposableAwaiter awaiter)
        {
            _operations ??= new HashSet<IDisposableAwaiter>();
            _operations.Add(awaiter);
            awaiter.OnCompleted(() => _operations?.Remove(awaiter));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Framework.Logging;
using Framework.Reactive;
using UniRx;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Framework.Addressables
{
    public class DependenciesLoader : BaseDisposable
    {
        private readonly ReactiveTrigger<long> _bytesLoadedDelta;
        private readonly ReactiveProperty<double> _downloadRateBytes;
        private readonly ReactiveProperty<bool> _isDone;
        private readonly ReactiveTrigger _onUpdateState;
        private long _downloadTotalBytes;
        private long _downloadedBytes;
        private IDisposable _timer;
        private DateTime _oldCalculateDateTime;
        private AsyncOperationHandle _asyncOperationHandle;
        private readonly HashSet<string> _downloadingKeys;
        private bool _isAsyncOperationHandleCreated;

        public long DownloadedBytes => _downloadedBytes;
        public IReadOnlyReactiveProperty<double> DownloadRateBytes => _downloadRateBytes;
        public long DownloadTotalBytes => _downloadTotalBytes;

        public IReadOnlyReactiveProperty<bool> IsDone => _isDone;
        public IReadOnlyReactiveTrigger OnUpdateState => _onUpdateState;
        
        public DependenciesLoader()
        {
            _downloadingKeys = new HashSet<string>();
            _bytesLoadedDelta = AddUnsafe(new ReactiveTrigger<long>());
            _downloadRateBytes = AddUnsafe(new ReactiveProperty<double>());
            _isDone = AddUnsafe(new ReactiveProperty<bool>());
            _onUpdateState = AddUnsafe(new ReactiveTrigger());
        }

        public async Task DownloadAsync()
        {
            const int TIMER_UPDATE_INTERVAL_MILLISECONDS = 100;
            _timer = Observable.Interval(TimeSpan.FromMilliseconds(TIMER_UPDATE_INTERVAL_MILLISECONDS))
                .Subscribe(_ =>
                {
                    CalculateState();
                });
            try
            {
                long downloadTotalBytes = 0;
                ProcessDownload();

                await _asyncOperationHandle.Task;

                CalculateState();
                
                Debug.Log($"DependenciesLoader, DownloadAsync, download complete, download total bytes: {downloadTotalBytes}, downloaded bytes: {_downloadedBytes}");
            }
            finally
            {
                _timer.Dispose();
                _timer = null;

                bool isOperationDone = _asyncOperationHandle.Status != AsyncOperationStatus.Failed;

                _isDone.Value = isOperationDone;

                _downloadingKeys.Clear();

                UnityEngine.AddressableAssets.Addressables.Release(_asyncOperationHandle);

                Debug.Log($"Done");
            }
        }

        public void AddToDownload(string key)
        {
            _downloadingKeys.Add(key);
        }

        public void AddToDownload(IEnumerable<AssetReference> references)
        {
            foreach (AssetReference reference in references)
            {
                if (!reference.RuntimeKeyIsValid())
                {
                    Debug.LogWarning($"DependenciesLoader, AddToDownload, asset key: {reference.AssetGUID}, runtime key isn't valid");

                    continue;
                }

                string key = reference.AssetGUID;
                //Debug.Log($"DependenciesLoader, AddToDownload, key: {key}, {reference.SubObjectName}");
                _downloadingKeys.Add(key);
            }
        }
        
        protected override void OnDispose()
        {
            _timer?.Dispose();
        }

        private void ProcessDownload()
        {
            if (_downloadingKeys.Count == 0)
            {
                Debug.Log("DependenciesLoader, ProcessDownload, can't process download, downloading keys is empty");
                return;
            }

            AsyncOperationHandle handle = UnityEngine.AddressableAssets.Addressables.DownloadDependenciesAsync(_downloadingKeys, UnityEngine.AddressableAssets.Addressables.MergeMode.Union, false);
            if (!handle.IsValid())
            {
                Debug.LogWarning("DependenciesLoader, DownloadDependenciesAsync, handle isn't valid");

                return;
            }

            switch (handle.Status)
            {
                case AsyncOperationStatus.Failed:
                    Debug.LogError("DownloadDependenciesAsync, handle status failed");

                    return;

                case AsyncOperationStatus.Succeeded:
                    Debug.Log("DownloadDependenciesAsync, handle status success");

                    return;

                case AsyncOperationStatus.None:
                    Debug.Log("DownloadDependenciesAsync, register handle");

                    DownloadStatus downloadStatus = handle.GetDownloadStatus();
                    if (downloadStatus.TotalBytes != 0)
                    {
                        _downloadTotalBytes = downloadStatus.TotalBytes;
                    }

                    _oldCalculateDateTime = DateTime.Now;
                    _asyncOperationHandle = handle;
                    _isAsyncOperationHandleCreated = true;
                    CalculateState();
                    
                    break;
            }
        }

        private void CalculateState()
        {
            if (!_isAsyncOperationHandleCreated)
            {
                return;
            }

            DateTime dateTimeNow = DateTime.Now;

            long totalDownloadedBytes = 0;
            
            DownloadStatus downloadStatus = _asyncOperationHandle.GetDownloadStatus();
            if (downloadStatus.TotalBytes != 0)
            {
                _downloadTotalBytes = downloadStatus.TotalBytes;
            }

            totalDownloadedBytes = downloadStatus.DownloadedBytes;

            long bytesLoadedDelta = totalDownloadedBytes - _downloadedBytes;
            if (bytesLoadedDelta > 0)
            {
                _bytesLoadedDelta.Notify(bytesLoadedDelta);
            }

            double elapsedSeconds = (dateTimeNow - _oldCalculateDateTime).TotalSeconds;
            if (_oldCalculateDateTime != default && elapsedSeconds > 0)
            {
                // avg
                double downloadRateBytes = (bytesLoadedDelta / elapsedSeconds + _downloadRateBytes.Value) / 2.0;
                _downloadRateBytes.Value = downloadRateBytes;
            }

            _oldCalculateDateTime = dateTimeNow;
            _downloadedBytes = totalDownloadedBytes;
            _onUpdateState?.Notify();
        }
    }
}
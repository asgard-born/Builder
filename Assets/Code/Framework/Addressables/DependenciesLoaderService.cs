using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Framework.Logging;
using UniRx;
using UnityEngine.ResourceManagement.Exceptions;

namespace Framework.Addressables
{
    public class DependenciesLoaderService : BaseDisposable
    {
        public struct Ctx
        {
            public ReactiveProperty<long> downloadedBytes;
            public ReactiveProperty<long> downloadTotalBytes;
            public ReactiveProperty<double> downloadRateBytes;
            public ReactiveProperty<int> progressPercentage;
        }

        private readonly Ctx _ctx;
        private bool _isDownloadComplete;

        private readonly List<DependenciesLoader> _loaders = new();

        public DependenciesLoaderService(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void AddToDownload(DependenciesLoader dependenciesLoader)
        {
            AddUnsafe(dependenciesLoader.OnUpdateState.Subscribe(OnUpdateState));
            AddUnsafe(ObservableExtensions.Subscribe(dependenciesLoader.DownloadRateBytes, OnDownloadRateChanged));

            _loaders.Add(dependenciesLoader);
        }

        public async Task DownloadAsync()
        {
            if (_isDownloadComplete)
            {
                Debug.LogError("DependenciesLoaderService, DownloadAsync can be call only once");
            }

            List<Task> downloadTasks =
                _loaders.Select(dependenciesLoader => dependenciesLoader.DownloadAsync()).ToList();


            await Task.WhenAll(downloadTasks);

            bool isAllDone = _loaders.Aggregate(true, (current, loader) => current & loader.IsDone.Value);

            _isDownloadComplete = true;

            Debug.Log(
                $"DependenciesLoaderService, DownloadAsync, total bytes to load: {_ctx.downloadTotalBytes.Value}, downloaded: {_ctx.downloadedBytes}");

            if (!isAllDone)
            {
                throw new OperationException("Download error, all tasks is not done");
            }
        }

        protected override void OnDispose()
        {
            foreach (DependenciesLoader dependenciesLoader in _loaders)
            {
                dependenciesLoader?.Dispose();
            }

            _loaders.Clear();
        }

        private void OnUpdateState()
        {
            long downloadedBytes = 0;
            long totalBytes = 0;
            
            foreach (DependenciesLoader loader in _loaders)
            {
                downloadedBytes += loader.DownloadedBytes;
                totalBytes += loader.DownloadTotalBytes;
            }

            _ctx.downloadedBytes.Value = Math.Max(_ctx.downloadedBytes.Value, downloadedBytes);
            _ctx.downloadTotalBytes.Value = Math.Max(_ctx.downloadTotalBytes.Value, totalBytes);
            
            _ctx.progressPercentage.Value = (int)(totalBytes == 0
                ? 0
                : 100 * downloadedBytes / totalBytes);
        }

        private void OnDownloadRateChanged(double downloadRateBytes)
        {
            _ctx.downloadRateBytes.Value = downloadRateBytes;
        }
    }
}
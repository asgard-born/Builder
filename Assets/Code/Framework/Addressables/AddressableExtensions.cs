using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = Framework.Logging.Debug;

namespace Framework.Addressables
{
    public static class AddressableExtensions
    {
        public static async Task<(T asset, AddressableRetain release)> TryLoadAsync<T>(this string address)
        {
            Debug.Log($"AddressableExtensions, load asset by address: {address}");
            
            return await LoadAssetAsync<T>(address);
        }

        public static async Task<(T asset, AddressableRetain release)> TryLoadAsync<T>(this AssetReference reference)
        {
            if (reference == null)
            {
                Debug.LogError("reference can't be null");
                return default;
            }

            try
            {
                Debug.Log($"AddressableExtensions, load asset by reference: {reference.SubObjectName}");
                return await LoadAssetAsync<T>(reference);
            }
            finally
            {
                //float endTime = Time.realtimeSinceStartup;
                //float duration = endTime - startTime;
                //Debug.Log($"ADDRESSABLES: {refName} | time: {startTime:0.00} -> {duration:0.00} -> {endTime:0.00}");
            }
        }

        public static Task<(GameObject go, AddressableRetain release)> TryLoadGameObjAsync(this AssetReference reference)
            => reference.TryLoadAsync<GameObject>();

        public static async Task<(T comp, AddressableRetain release)> TryLoadGameObjAsync<T>(this AssetReference reference)
            where T : class
        {
            (GameObject obj, AddressableRetain retain) = await reference.TryLoadGameObjAsync();
            if (!obj)
            {
                retain.Dispose();
                return (null, default);
            }
            T comp = obj.GetComponent<T>();
            if (comp == null)
            {
                retain.Dispose();
                Debug.LogError($"can't find component of type {typeof(T).Name}" +
                               $" on instantiated addressable asset {reference.SubObjectName}");
                return (null, default);
            }
            return (comp, retain);
        }

        private static async Task<(T asset, AddressableRetain release)> LoadAssetAsync<T>(object key)
        {
            const int RETRY_INTERVAL_SECONDS = 5;
            while (true)
            {
                try
                {
                    AsyncOperationHandle<T> handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(key);
                    T asset = await handle.Task;
                    if (handle.OperationException != null)
                    {
                        Debug.LogError($"can't instantiate addressable asset {key} by exception");
                        Debug.LogException(handle.OperationException);
                        UnityEngine.AddressableAssets.Addressables.Release(handle);
                        
                        await Task.Delay(TimeSpan.FromSeconds(RETRY_INTERVAL_SECONDS));
                        continue;
                    }
                    if (!handle.IsDone)
                    {
                        Debug.LogError($"can't instantiate addressable asset {key}, it's undone");
                        UnityEngine.AddressableAssets.Addressables.Release(handle);
                        
                        await Task.Delay(TimeSpan.FromSeconds(RETRY_INTERVAL_SECONDS));
                        continue;
                    }
                    if (asset == null)
                    {
                        Debug.LogError($"can't instantiate addressable asset {key}");
                        UnityEngine.AddressableAssets.Addressables.Release(handle);
                        
                        await Task.Delay(TimeSpan.FromSeconds(RETRY_INTERVAL_SECONDS));
                        continue;
                    }
                    else
                    {
                        //refName = asset.ToString();
                    }

                    return (asset, new AddressableRetain(handle));
                }
                catch (Exception e)
                {
                    await Task.Delay(TimeSpan.FromSeconds(RETRY_INTERVAL_SECONDS));
                }
            }
        }
    }
}
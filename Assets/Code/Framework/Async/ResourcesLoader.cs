using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Async
{
    public static class ResourcesLoader
    {
        public static async Task<T> LoadAsync<T>(string assetName) where T : Object
        {
            ResourceRequest resource = Resources.LoadAsync<T>(assetName);
            while (!resource.isDone)
            {
                await Task.Yield();
            }

            return resource.asset as T;
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Debug;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utils
{
    public static class AddressablesSystem<T>
    {
        private static readonly Dictionary<string, T> LoadedAssets = new();
        private static readonly Dictionary<string, AsyncOperationHandle<T>> CurrentlyLoadingHandles = new();

        public static async Task<T> GetOrLoadAddressable(string addressableId)
        {
            //If we've already loaded the addressable then return it instantly
            if (LoadedAssets.TryGetValue(addressableId, out var addressable))
            {
                return addressable;
            }

            //Load addressable Asynchronously
            if (!CurrentlyLoadingHandles.TryGetValue(addressableId, out var handle))
            {
                //Failed to get in dictionary, so this is the first time we are trying to load
                handle = Addressables.LoadAssetAsync<T>(
                    addressableId);
                CurrentlyLoadingHandles.Add(addressableId, handle);
            }

            //Await until done
            while (!handle.IsDone)
            {
                await Task.Yield();
            }

            //Success, save result
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                LoadedAssets.Add(addressableId, handle.Result);
            }
            else
            {
                //Failed to load
                handle.Release();
                DebugSystem.Error($"Failed to load addressable {addressableId}!");
            }

            //Return asset or null
            return handle.Result;
        }
    }
}
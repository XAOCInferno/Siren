using System.Collections.Generic;
using System.Threading.Tasks;
using Debug;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utils
{
    /// <summary>
    /// A global system to grab data from addressables. Will ensure that addressables are only fetched once.
    /// </summary>
    /// <typeparam name="T">The asset type that we will be grabbing from addressables eg GameObject, Material, etc.</typeparam>
    public static class AddressablesSystem<T>
    {
        private static readonly Dictionary<string, T> LoadedAssets = new();
        private static readonly Dictionary<string, AsyncOperationHandle<T>> CurrentlyLoadingHandles = new();
        private static readonly List<string> ActivelyLoadingAddressables = new();

        /// <summary>
        /// Returns either the cached addressable result or fetches it from the addressables.
        /// </summary>
        /// <param name="addressableId">The name of the addressable defined on the object in Unity</param>
        /// <returns></returns>
        public static async Task<T> GetOrLoadAddressable(string addressableId)
        {
            // Wait until finished loading a previous request before continuing
            while (ActivelyLoadingAddressables.Contains(addressableId))
            {
                await Task.Yield();
            }

            // If we've already loaded the addressable then return it instantly
            if (LoadedAssets.TryGetValue(addressableId, out var addressable))
            {
                return addressable;
            }

            // Load addressable Asynchronously
            if (!CurrentlyLoadingHandles.TryGetValue(addressableId, out var handle))
            {
                // Failed to get in dictionary, so this is the first time we are trying to load

                // Track we're loading so that we don't get duplicate load when called in multiple places simultaneously
                ActivelyLoadingAddressables.Add(addressableId);

                // Begin async load
                handle = Addressables.LoadAssetAsync<T>(
                    addressableId);
                CurrentlyLoadingHandles.Add(addressableId, handle);
            }

            //Await until done
            while (!handle.IsDone)
            {
                await Task.Yield();
            }

            // Remove our loading tracking
            int idx = ActivelyLoadingAddressables.FindIndex(x => x.Equals(addressableId));
            if (idx > -1)
            {
                ActivelyLoadingAddressables.RemoveAt(idx);
            }

            // Success, save result
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                LoadedAssets.Add(addressableId, handle.Result);
            }
            else
            {
                // Failed to load
                handle.Release();
                DebugSystem.Error($"Failed to load addressable {addressableId}!");
            }

            // Return asset or null
            return handle.Result;
        }
    }
}
using System;
using System.Collections.Generic;
using Debug;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using Object = System.Object;

namespace Utils
{
    public abstract class PooledObject : MonoBehaviour
    {
        private Type _poolType;

        public abstract void SetActive();
        public abstract void SetInActive();

        public Type GetPoolType()
        {
            return _poolType;
        }

        public void SetPoolType(Type type)
        {
            _poolType = type;
        }
    }

    public class Pool<T> where T : PooledObject
    {
        //Inactive objects
        private readonly List<T> _available = new();

        //Active objects
        private readonly List<T> _active = new();

        //ID for pool to help with logging and searching
        private readonly Type _poolType = typeof(T);

        //Get ID
        public Type GetPoolType()
        {
            return _poolType;
        }

        //[available->active] Get the next available object in the pool
        [CanBeNull]
        public PooledObject GetNextAvailable()
        {
            //Check we have anything to get
            if (_available.Count == 0)
            {
                DebugSystem.Warn($"Cannot get available pooled object of {_poolType}, consider resizing the pool");
                return null;
            }

            //Update data, it's now active
            T obj = _available[0];
            _available.RemoveAt(0);
            _active.Add(obj);
            //Set State on object
            obj.SetActive();
            return obj;
        }

        //[active->available] Return an object back to the pool
        public void ReturnToPool(T objToReturn)
        {
            //Find if card is active, we can only return active objects
            int idx = _active.FindIndex((v) => v == objToReturn);
            if (idx == 0)
            {
                DebugSystem.Warn(
                    $"Cannot return object {objToReturn.GetPoolType()} to pool {_poolType} as object is not active");
                return;
            }

            //Add to data
            _active.RemoveAt(idx);
            _available.Add(objToReturn);
            //Set State on object
            objToReturn.SetInActive();
        }

        //[external->available] Add an object to the pool
        public void AddToPool(T objToAdd)
        {
            //Check if object is already in the pool, we can't add it twice
            int idxActive = _active.FindIndex((v) => v == objToAdd);
            int idxAvailable = _available.FindIndex((v) => v == objToAdd);
            if (idxActive > 0 || idxAvailable > 0)
            {
                DebugSystem.Warn(
                    $"Trying to add object {objToAdd.GetPoolType()} to pool {_poolType}, but object is already in the pool!");
                return;
            }

            //Add to data
            _available.Add(objToAdd);
            //Set State on object
            objToAdd.SetInActive();
        }

        //Get size
        public int GetPoolSize()
        {
            return _active.Count + _available.Count;
        }
    }

    public static class PoolSystem<T> where T : PooledObject
    {
        private static readonly Pool<T> Pool = new Pool<T>();
        private static GameObject _templateToInstantiate;
        private static bool _isPoolReady;

        //Gets existing pool of ID or get 
        public static Pool<T> GetPool()
        {
            return Pool;
        }

        public static bool GetIsPoolReady() => _isPoolReady;

        public static void SetTemplateToInstantiate(GameObject template)
        {
            _templateToInstantiate = template;
        }

        public static void SetPoolSize(int size)
        {
            if (!_templateToInstantiate)
            {
                DebugSystem.Error("Attempting to set pool size, but template to instantiate is undefined.");
                return;
            }

            //Check current size
            int currentPoolSize = Pool.GetPoolSize();
            if (currentPoolSize > 0)
            {
                //Get if we should resize
                if (currentPoolSize < size)
                {
                    DebugSystem.Warn(
                        $"Pool {Pool.GetType()} size is already set though it's lower than desired, resizing. Though we should not be dynamically resizing.");
                }
                else
                {
                    DebugSystem.Warn("Pool size is already larger than desired, will not resize");
                    return;
                }
            }

            //Set pool size
            for (int i = 0; i < size - currentPoolSize; i++)
            {
                GameObject newObject = PoolHelper.InstantiatePooledObject(_templateToInstantiate);
                T component = newObject.GetComponent<T>();
                if (!component)
                {
                    DebugSystem.Error($"Cannot get component {typeof(T)} from instantiated object {newObject}");
                    return;
                }

                Pool.AddToPool(component);
            }

            //We've setup pool, save that
            _isPoolReady = true;
        }
    }

    public class PoolHelper : MonoBehaviour
    {
        public static GameObject InstantiatePooledObject(GameObject template)
        {
            GameObject newObject = Instantiate(template);
            newObject.name = $"PooledObject-{template.name}";
            return newObject;
        }
    }
}
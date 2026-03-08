using System.Collections.Generic;
using Debug;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace Utils
{
    public enum EPoolIDs
    {
        Card = 0,
    }

    public abstract class PooledObject : MonoBehaviour
    {
        private EPoolIDs _id;

        public abstract void SetActive();
        public abstract void SetInActive();

        public EPoolIDs GetId()
        {
            return _id;
        }

        public void SetId(EPoolIDs id)
        {
            _id = id;
        }
    }

    public class Pool<T> where T : PooledObject
    {
        //Inactive objects
        private readonly List<T> _available = new();

        //Active objects
        private readonly List<T> _active = new();

        //ID for pool to help with logging and searching
        private readonly EPoolIDs _poolID;

        //Constructor, set our ID
        public Pool(EPoolIDs poolID)
        {
            _poolID = poolID;
        }

        //Get ID
        public EPoolIDs GetPoolID()
        {
            return _poolID;
        }

        //[available->active] Get the next available object in the pool
        [CanBeNull]
        public PooledObject GetNextAvailable()
        {
            if (_available.Count == 0)
            {
                DebugSystem.Warn($"Cannot get available pooled object of {_poolID}, consider resizing the pool");
                return null;
            }

            T obj = _available[0];
            _available.RemoveAt(0);
            _active.Add(obj);
            return obj;
        }

        //[active->available] Return an object back to the pool
        public void ReturnToPool(T objToReturn)
        {
            int idx = _active.FindIndex((v) => v == objToReturn);
            if (idx == 0)
            {
                DebugSystem.Warn(
                    $"Cannot return object {objToReturn.GetId()} to pool {_poolID} as object is not active");
                return;
            }

            _active.RemoveAt(idx);
            _available.Add(objToReturn);
        }

        //[external->available] Add an object to the pool
        public void AddToPool(T objToAdd)
        {
            int idxActive = _active.FindIndex((v) => v == objToAdd);
            int idxAvailable = _available.FindIndex((v) => v == objToAdd);
            if (idxActive > 0 || idxAvailable > 0)
            {
                DebugSystem.Warn(
                    $"Trying to add object {objToAdd.GetId()} to pool {_poolID}, but object is already in the pool!");
                return;
            }

            _available.Add(objToAdd);
        }

        //Get size
        public int GetPoolSize()
        {
            return _active.Count + _available.Count;
        }
    }

    public static class PoolSystem<T> where T : PooledObject
    {
        private static readonly List<Pool<T>> Pools = new();

        //Gets existing pool of ID or get 
        public static Pool<T> GetPool(EPoolIDs poolID)
        {
            //Try get existing 
            Pool<T> pool = Pools.Find((v) => v.GetPoolID() == poolID);
            if (pool != null) return pool;
            //No existing so add
            pool = new Pool<T>(poolID);
            Pools.Add(pool);
            return pool;
        }

        public static void SetPoolSize(int size, EPoolIDs poolID)
        {
            //Get pool
            Pool<T> pool = GetPool(poolID);
            //Check current size
            int currentPoolSize = pool.GetPoolSize();
            if (currentPoolSize > 0)
            {
                //Get if we should resize
                if (currentPoolSize < size)
                {
                    DebugSystem.Warn(
                        $"Pool {poolID} size is already set though it's lower than desired, resizing. Though we should not be dynamically resizing.");
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
                pool.AddToPool(PoolHelper.InstantiatePooledObject<T>());
            }
        }
    }

    public class PoolHelper : MonoBehaviour
    {
        public static T InstantiatePooledObject<T>() where T : PooledObject
        {
            return Instantiate(new GameObject()).GetOrAddComponent<T>();
        }
    }
}
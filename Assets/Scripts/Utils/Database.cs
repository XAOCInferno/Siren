using System;
using Debug;
using UnityEngine;

namespace Utils
{
    public static class Database<T>
    {
        private static T[] _items;
        private static bool _isDatabaseSetup;

        public static T[] GetItems() => _items;
        public static bool GetIsDatabaseSet() => _isDatabaseSetup;

        public static void SetItem(T[] newItems)
        {
            if (_items is { Length: > 0 })
            {
                DebugSystem.Warn("Attempting to set card database again after already setting it. Will clear first");
            }

            _items = newItems;
            _isDatabaseSetup = true;
        }

        public static void ClearItems()
        {
            _isDatabaseSetup = false;
            _items = Array.Empty<T>();
        }
    }

    public class DatabaseStorage<T> : MonoBehaviour
    {
        [SerializeField] private T[] items;

        private void Awake()
        {
            Database<T>.SetItem(items);
        }
    }
}
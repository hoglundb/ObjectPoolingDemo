using System.Collections.Generic;
using UnityEngine;

namespace Code.Pooling
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance;

        private void Awake() => Instance = this;

        private Dictionary<string, object> pools = new();

        public T Get<T>(T prefab, int initialSize = 10) where T : PoolableObject
        {
   
            string key = prefab.name;

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("PrefabIdentity component is missing a prefab Id: " + prefab.name);
                return null;
            }

            if (!pools.ContainsKey(key))
                pools[key] = new ObjectPool<T>(prefab, initialSize);

            return ((ObjectPool<T>)pools[key]).Get();
        }

        public void Return<T>(T obj) where T : PoolableObject
        {
            if (obj == null)
            {
                Debug.LogError("Trying to return null object");
                return;
            }

            var key = obj.name;

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("PrefabIdentity component is missing a prefab Id: " + obj.name);
                return;
            }

            if (pools.ContainsKey(key))
            {
                ((ObjectPool<T>)pools[key]).Return(obj);
                return;
            }
            Debug.LogError("Trying to return object to non-existent pool: " + key + " obj name: " + obj.name);
        }
    }
}
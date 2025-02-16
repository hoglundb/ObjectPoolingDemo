using System.Collections.Generic;
using UnityEngine;

namespace Code.Pooling
{
    public class ObjectPool<T> where T : PoolableObject
    {
        private List<T> pool = new List<T>();
        private T prefab;
        private Transform poolParent;

        public ObjectPool(T prefab, int initialSize, Transform parent = null)
        {
            this.prefab = prefab;

            if (parent == null)
            {
                var poolObject = new GameObject(prefab.name + " Pool");
                poolParent = poolObject.transform;
            }
            else poolParent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                T obj = Object.Instantiate(prefab, poolParent);
                obj.gameObject.name = prefab.name;
                obj.IsInPool = true;
                obj.OnDespawn();
                pool.Add(obj);
            }
        }

        public T Get()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && pool[i].IsInPool)
                {
                    pool[i].IsInPool = false;
                    pool[i].OnSpawn();
                    return pool[i];
                }
            }

            T newObj = Object.Instantiate(prefab, poolParent);
            newObj.name = prefab.name;
            newObj.gameObject.name = prefab.name;
            newObj.IsInPool = false;
            newObj.OnSpawn();
            pool.Add(newObj);
            return newObj;
        }

        public void Return(T obj)
        {
            if (obj == null)
            {
                Debug.LogError("Trying to return null object to pool");
                return;
            }
            obj.IsInPool = true;
            obj.OnDespawn();
        }
    }
}
using UnityEngine;

    public abstract class PoolableObject : MonoBehaviour
    {

        public bool IsInPool = false;

        public abstract void OnSpawn();
        public abstract void OnDespawn();
    }
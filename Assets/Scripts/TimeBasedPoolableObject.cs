using UnityEngine;

namespace Code.Pooling
{
    public class TimeBasedPoolableObject : PoolableObject
    {
        [Space]
        [SerializeField] private float lifeTimeDuration = 10;

        private float timeSinceSpawned;

        private void OnValidate() => enabled = false;

        public override void OnSpawn()
        {
            timeSinceSpawned = 0;
            enabled = true;
            gameObject.SetActive(true);
        }

        public override void OnDespawn()
        {
            enabled = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            timeSinceSpawned += Time.deltaTime;
            if (timeSinceSpawned < lifeTimeDuration) return;
            ReturnToPool();
        }

        private void ReturnToPool() => PoolManager.Instance.Return(this);
    }
}
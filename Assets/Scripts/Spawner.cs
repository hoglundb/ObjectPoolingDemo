using Code.Pooling;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private TimeBasedPoolableObject objectPrefab;
    [SerializeField] private float spawnPositionRandomization;

    public void SpawnObject()
    {
        var obj = PoolManager.Instance.Get(objectPrefab);
        obj.transform.position = GetRandomSpawnPosition();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(-spawnPositionRandomization, spawnPositionRandomization);
        float randomY = Random.Range(-spawnPositionRandomization, spawnPositionRandomization);
        float randomZ = Random.Range(-spawnPositionRandomization, spawnPositionRandomization);

        return new Vector3(randomX, randomY, randomZ) + transform.position;
    }
}

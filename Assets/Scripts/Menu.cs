using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Menu : MonoBehaviour
    {
        [Space]
        [SerializeField] private Spawner sphereSpawner;
        [SerializeField] private Spawner cubeSpawner;

        [Space]
        [SerializeField] private Button spawnSphereButton;
        [SerializeField] private Button spawnCubeButton;

        private void Awake()
        {
            spawnSphereButton.onClick.AddListener(() => sphereSpawner.SpawnObject());

            spawnCubeButton.onClick.AddListener(() => cubeSpawner.SpawnObject());
        }
    }
}
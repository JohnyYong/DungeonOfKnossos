using System.Collections.Generic;
using UnityEngine;

public class RandomAppleSpawner : MonoBehaviour
{
    [Header("Apple Prefabs")]
    public List<GameObject> applePrefabs;

    [Header("Spawn Position")]
    public Transform spawnPoint;

    void Start()
    {
        SpawnRandomApple();
    }

    void SpawnRandomApple()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("[AppleSpawner] No spawn point assigned!");
            return;
        }

        if (applePrefabs == null || applePrefabs.Count == 0)
        {
            Debug.LogWarning("[AppleSpawner] Apple prefab list is empty!");
            return;
        }

        int randomIndex = Random.Range(0, applePrefabs.Count);
        GameObject chosenApple = applePrefabs[randomIndex];

        if (chosenApple != null)
        {
            Instantiate(chosenApple, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"[AppleSpawner] Apple prefab at index {randomIndex} is null!");
        }
    }
}

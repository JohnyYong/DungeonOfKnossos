using UnityEngine;

public class MonsterHouseTrigger : MonoBehaviour
{
    public EnemyBuckets enemyBucket;
    public int totalEnemiesToSpawn = 6;
    private bool triggered = false;

    public GameObject dropEffectPrefab;
    public float dropRadius = 6f;
    public void TriggerAmbush()
    {
        if (triggered || enemyBucket == null) return;
        triggered = true;

        Vector3 center = transform.position;
        for (int i = 0; i < totalEnemiesToSpawn; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * dropRadius;
            Vector3 spawnPos = center + (Vector3)offset;

            GameObject prefab = (Random.value < 0.5f && enemyBucket.trivialEnemies.Count > 0)
                ? enemyBucket.trivialEnemies[Random.Range(0, enemyBucket.trivialEnemies.Count)]
                : enemyBucket.easyEnemies[Random.Range(0, enemyBucket.easyEnemies.Count)];

            GameObject drop = Instantiate(dropEffectPrefab, spawnPos, Quaternion.identity);
            drop.GetComponent<DropSpawnEffect>().enemyToSpawn = prefab;
        }

        Debug.Log("[MonsterHouse] Ambush triggered!");
        Destroy(gameObject);
    }

}

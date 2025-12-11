using System.Collections;
using UnityEngine;

public class SpiderEnemy : MonoBehaviour
{
    public GameObject webPrefab;
    public float webSpawnIntervalMin = 2f;
    public float webSpawnIntervalMax = 5f;
    public int maxWebsToSpawn = 2;

    private int websSpawned = 0;
    private bool spawningStarted = false;

    void Update()
    {
        EnemyChaseLogic chaseLogic = GetComponent<EnemyChaseLogic>();
        if (chaseLogic != null && chaseLogic.Aggroed && !spawningStarted)
        {
            spawningStarted = true;
            StartCoroutine(SpawnWebRoutine());
        }
    }

    IEnumerator SpawnWebRoutine()
    {
        while (websSpawned < maxWebsToSpawn)
        {
            float waitTime = Random.Range(webSpawnIntervalMin, webSpawnIntervalMax);
            yield return new WaitForSeconds(waitTime);

            Instantiate(webPrefab, transform.position, Quaternion.identity);
            websSpawned++;
        }
    }
}

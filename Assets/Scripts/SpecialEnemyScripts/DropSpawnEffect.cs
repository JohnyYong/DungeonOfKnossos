using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class DropSpawnEffect : MonoBehaviour
{
    public GameObject enemyToSpawn;
    public float dropDistance = 5f;
    public float dropDuration = 0.5f;

    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;
        transform.position += Vector3.up * dropDistance;
        StartCoroutine(Drop());
    }

    System.Collections.IEnumerator Drop()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        Instantiate(enemyToSpawn, transform.position, Quaternion.identity);

        // Instantiate(landingEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}

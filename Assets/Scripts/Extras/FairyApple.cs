using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FairyApple : MonoBehaviour
{
    [Tooltip("Assign the specific fairy companion prefab this apple spawns")]
    public GameObject fairyCompanionPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && fairyCompanionPrefab != null)
        {
            Instantiate(fairyCompanionPrefab, other.transform.position, Quaternion.identity, other.transform);
            Destroy(gameObject);
        }
    }
}

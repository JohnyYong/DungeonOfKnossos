using System.Collections;
using UnityEngine;

public class ReaperTeleport : MonoBehaviour
{
    [Header("Teleport Animation Settings")]
    public float shrinkTime = 0.5f;
    public float growTime = 0.5f;
    public float spinSpeed = 720f;

    public bool isTeleporting = false;
    private Transform waypointsParent;
    private EnemyChaseLogic chaseLogic;
    [Header("Teleport Effects")]
    public GameObject arrivalEffectPrefab;
    public float preArrivalDelay = 0.3f;

    void Start()
    {
        chaseLogic = GetComponent<EnemyChaseLogic>();
    }

    public void SetWaypointsParent(Transform wp)
    {
        waypointsParent = wp;
    }

    public void TeleportToRandomWaypoint()
    {
        if (!isTeleporting && waypointsParent != null)
            StartCoroutine(TeleportRoutine());
    }

    IEnumerator TeleportRoutine()
    {
        isTeleporting = true;

        if (chaseLogic != null)
            chaseLogic.enabled = false;

        Vector3 originalScale = transform.localScale;
        float t = 0f;

        // Shrinking phase
        while (t < shrinkTime)
        {
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t / shrinkTime);
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero;

        // Get waypoint
        Transform[] children = waypointsParent.GetComponentsInChildren<Transform>();
        Transform chosen = null;

        if (children.Length > 1)
        {
            chosen = children[Random.Range(1, children.Length)];

            // Spawn particle effect at destination
            if (arrivalEffectPrefab != null)
                Instantiate(arrivalEffectPrefab, chosen.position, Quaternion.identity);
        }

        // Wait briefly to build anticipation
        yield return new WaitForSeconds(preArrivalDelay);

        // Move to destination
        if (chosen != null)
            transform.position = chosen.position;

        // Growing phase
        t = 0f;
        while (t < growTime)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t / growTime);
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;

        if (chaseLogic != null)
            chaseLogic.enabled = true;

        isTeleporting = false;
    }

}

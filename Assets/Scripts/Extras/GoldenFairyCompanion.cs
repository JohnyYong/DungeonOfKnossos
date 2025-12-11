using System.Collections;
using UnityEngine;

public class GoldenFairyCompanion : MonoBehaviour
{
    [Header("Orbit Settings")]
    public Transform player;
    public float orbitRadius = 1.5f;
    public float orbitSpeed = 180f; // degrees per second
    private float currentAngle = 0f;

    [Header("Ability Settings")]
    public GameObject swordPrefab;
    public float abilityCooldown = 6f;
    private bool isOnCooldown = false;

    [Header("VFX Settings")]
    public GameObject summonEffectPrefab;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        UiStatsDisplay.Instance.ShowFairyMessage("Press E to summon a divine sword!");
    }

    void Update()
    {
        OrbitAroundPlayer();

        if (Input.GetKeyDown(KeyCode.E) && !isOnCooldown)
        {
            StartCoroutine(TriggerFairySword());
        }
    }

    void OrbitAroundPlayer()
    {
        if (player == null) return;

        currentAngle += orbitSpeed * Time.unscaledDeltaTime;
        if (currentAngle >= 360f)
            currentAngle -= 360f;

        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
        transform.position = player.position + offset;

        Vector3 direction = (transform.position - player.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    IEnumerator TriggerFairySword()
    {
        isOnCooldown = true;

        // Summon VFX
        if (summonEffectPrefab)
        {
            Instantiate(summonEffectPrefab, player.position, Quaternion.identity);
        }

        // Spawn sword
        if (swordPrefab)
        {
            Instantiate(swordPrefab, player.position + new Vector3(0, -2f, 0), Quaternion.identity);
        }

        yield return new WaitForSecondsRealtime(abilityCooldown);
        isOnCooldown = false;
    }
}

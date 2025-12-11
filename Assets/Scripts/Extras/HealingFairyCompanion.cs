using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingFairyCompanion : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;

    [Header("Healing Settings")]
    public float baseHealInterval = 2f;   // Minimum interval when HP is 0
    public float maxHealDelay = 5f;       // Maximum delay when HP is full
    public int healAmount = 1;
    public GameObject healEffectPrefab;

    private HeroStats heroStats;
    private Coroutine healingRoutine;

    [Header("Orbit Settings")]
    public float orbitRadius = 1.5f;
    public float orbitSpeed = 180f;

    private float currentAngle = 0f;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        heroStats = player.GetComponent<HeroStats>();
        UiStatsDisplay.Instance.ShowFairyMessage("The Healing Fairy is now supporting you!");

        healingRoutine = StartCoroutine(HealLoop());
    }

    void Update()
    {
        OrbitAroundPlayer();
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

    IEnumerator HealLoop()
    {
        while (true)
        {
            if (heroStats != null && heroStats.Health < heroStats.MaxHealth)
            {
                heroStats.Health += healAmount;

                if (healEffectPrefab)
                    Instantiate(healEffectPrefab, player.position, Quaternion.identity);
            }

            // Calculate delay based on health ratio
            float healthRatio = (float)heroStats.Health / heroStats.MaxHealth;

            // Lerp from baseHealInterval to maxHealDelay as health increases
            float scaledDelay = Mathf.Lerp(baseHealInterval, maxHealDelay, healthRatio);

            yield return new WaitForSecondsRealtime(scaledDelay);
        }
    }
}

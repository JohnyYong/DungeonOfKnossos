using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyChaseLogic))]
public class GiantSlime : MonoBehaviour
{
    [Header("Slime Spawn Settings")]
    public GameObject smallSlimePrefab;
    public Transform spawnPoint;
    public int maxSlimes = 10;
    private int slimeSpawned = 0;
    private float spawnTimer = 0f;
    private float nextSpawnDelay = 0.5f;

    [Header("Breathing Settings")]
    public float breatheSpeed = 1f;
    public float breatheAmount = 0.05f;
    private Vector3 originalScale;

    [Header("Damage Settings")]
    public int damageAmount = 2;
    public float knockbackForce = 2f;
    public float knockbackDuration = 0.5f;
    public float damageCooldown = 2f;

    private Dictionary<GameObject, float> lastHitTime = new Dictionary<GameObject, float>();

    private EnemyChaseLogic chaseLogic;

    void Start()
    {
        originalScale = transform.localScale;
        chaseLogic = GetComponent<EnemyChaseLogic>();
        nextSpawnDelay = 5f; // First spawn
    }

    void Update()
    {
        AnimateBreathing();

        if (chaseLogic != null && chaseLogic.Aggroed && slimeSpawned < maxSlimes)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= nextSpawnDelay)
            {
                SpawnSlime();
                slimeSpawned++;
                spawnTimer = 0f;
                nextSpawnDelay = 5f * (slimeSpawned + 1); // Exponential wait
            }
        }
    }

    void SpawnSlime()
    {
        if (smallSlimePrefab == null) return;

        Vector3 spawnPos = spawnPoint != null
            ? spawnPoint.position
            : transform.position + Random.insideUnitSphere * 0.5f;

        spawnPos.z = 0f;

        Instantiate(smallSlimePrefab, spawnPos, Quaternion.identity);
    }

    void AnimateBreathing()
    {
        float scaleOffset = Mathf.Sin(Time.time * breatheSpeed) * breatheAmount;
        transform.localScale = originalScale + Vector3.one * scaleOffset;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        HeroStats hero = collision.gameObject.GetComponent<HeroStats>();
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (hero == null || rb == null) return;

        if (hero.GodMode)
            return;

        float currentTime = Time.time;
        if (lastHitTime.ContainsKey(collision.gameObject) &&
            currentTime - lastHitTime[collision.gameObject] < damageCooldown)
            return;

        lastHitTime[collision.gameObject] = currentTime;

        Vector2 knockDir = (collision.transform.position - transform.position).normalized;
        StartCoroutine(ApplyKnockback(rb, knockDir));
        hero.Health -= damageAmount;
    }

    private IEnumerator ApplyKnockback(Rigidbody2D rb, Vector2 direction)
    {
        TopDownController controller = rb.GetComponent<TopDownController>();
        if (controller != null) controller.enabled = false;

        rb.linearVelocity = direction * knockbackForce;
        yield return new WaitForSeconds(knockbackDuration);

        if (controller != null) controller.enabled = true;
    }

    public void OnDestroy()
    {
        // Nothing to stop, since we no longer use a coroutine.
    }
}

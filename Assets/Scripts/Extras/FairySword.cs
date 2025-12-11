using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairySword : MonoBehaviour
{
    [Header("Rising Settings")]
    public AnimationCurve riseCurve;
    public float riseDuration = 1f;
    public Vector3 startOffset = new Vector3(0, -2, 0);
    public Vector3 endOffset = new Vector3(0, 1.5f, 0);
    public CameraShake.ShakePreset riseShake = CameraShake.ShakePreset.Small;

    [Header("Swing Settings")]
    public float swingDuration = 0.5f;
    public float swingRadius = 3f;
    public LayerMask enemyLayer;
    public CameraShake.ShakePreset swingShake = CameraShake.ShakePreset.Small;
    public float immunityTime = 0.5f;
    private Dictionary<EnemyStats, float> lastHitTime = new Dictionary<EnemyStats, float>();

    [Header("Despawn Settings")]
    public GameObject despawnParticles;
    public CameraShake.ShakePreset despawnShake = CameraShake.ShakePreset.Large;

    private Transform player;
    private Vector3 basePosition;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("No player found for FairySword.");
            Destroy(gameObject);
            return;
        }

        StartCoroutine(PerformSequence());
    }

    IEnumerator PerformSequence()
    {
        yield return StartCoroutine(Raise());
        yield return StartCoroutine(Swing());
        yield return StartCoroutine(Despawn());
    }

    IEnumerator Raise()
    {
        float timer = 0f;
        Vector3 startPos = player.position + startOffset;
        Vector3 endPos = player.position + endOffset;

        // Start continuous shaking
        CameraShake.Instance.shakePreset = riseShake;
        CameraShake.Instance.TriggerContinuousShake();

        while (timer < riseDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / riseDuration);
            float eased = riseCurve.Evaluate(t);

            transform.position = Vector3.LerpUnclamped(startPos, endPos, eased);
            yield return null;
        }

        transform.position = endPos;

        // Stop continuous shaking
        CameraShake.Instance.StopShake();
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Colliding with:" + other.gameObject.tag);

            EnemyStats enemy = other.gameObject.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                if (!lastHitTime.ContainsKey(enemy))
                {
                    lastHitTime[enemy] = Time.time;
                    int damage = enemy.StartingHealth / 2 + 1;
                    enemy.Health -= damage;
                }
            }
        }
    }

    IEnumerator Swing()
    {
        float timer = 0f;
        CameraShake.Instance.StopShake();
        CameraShake.Instance.StopAllCoroutines();

        CameraShake.Instance.shakePreset = swingShake;
        CameraShake.Instance.TriggerShake();

        while (timer < swingDuration)
        {
            float angle = Mathf.Lerp(0f, 360f, timer / swingDuration);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * swingRadius;

            transform.position = player.position + offset;

            // Optional: rotate sword visually
            transform.rotation = Quaternion.Euler(0, 0, angle + 90f);

            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator Despawn()
    {
        CameraShake.Instance.shakePreset = despawnShake;
        CameraShake.Instance.TriggerShake();

        if (despawnParticles != null)
        {
            Instantiate(despawnParticles, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
        yield return null;
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, swingRadius);
        }
    }
}

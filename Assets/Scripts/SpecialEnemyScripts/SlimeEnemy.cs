using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEnemy : MonoBehaviour
{
    public int damageAmount = 1;
    public float knockbackForce = 1.5f;
    public float knockbackDuration = 1.0f;
    public float damageCooldown = 1.5f;

    private Dictionary<GameObject, float> lastHitTime = new Dictionary<GameObject, float>();

    // Breathing
    public float breatheSpeed = 1.5f;
    public float breatheScaleAmount = 0.1f;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
        StartCoroutine(BreathingEffect());
    }

    private IEnumerator BreathingEffect()
    {
        while (true)
        {
            float t = 0f;
            while (t < Mathf.PI * 2f)
            {
                t += Time.deltaTime * breatheSpeed;
                float scaleOffset = Mathf.Sin(t) * breatheScaleAmount;
                transform.localScale = new Vector3(
                    originalScale.x + scaleOffset,
                    originalScale.y - scaleOffset,
                    originalScale.z
                );
                yield return null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        HeroStats hero = other.GetComponent<HeroStats>();
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        if (hero == null || rb == null) return;

        if (hero.GodMode)
            return;
        float currentTime = Time.time;

        if (lastHitTime.ContainsKey(other.gameObject) && currentTime - lastHitTime[other.gameObject] < damageCooldown)
            return;
        lastHitTime[other.gameObject] = currentTime;

        Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
        StartCoroutine(ApplyKnockback(rb, knockbackDir));
        hero.Health -= damageAmount;
    }

    private IEnumerator ApplyKnockback(Rigidbody2D rb, Vector2 direction)
    {
        var movementScript = rb.GetComponent<TopDownController>();
        if (movementScript != null) movementScript.enabled = false;

        rb.linearVelocity = direction * knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);

        if (movementScript != null) movementScript.enabled = true;
    }
}

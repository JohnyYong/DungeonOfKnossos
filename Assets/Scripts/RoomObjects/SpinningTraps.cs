using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpinningTraps : MonoBehaviour
{
    [Header("Spinning Settings")]
    public float rotationSpeed = 180f;

    [Header("Damage & Knockback")]
    public int damageAmount = 1;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HeroStats hero = other.GetComponent<HeroStats>();



        if (hero != null)
        {
            if (hero.GodMode)
                return;

            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                StartCoroutine(ApplyKnockback(rb, knockbackDir));
            }

            hero.Health -= damageAmount;
        }
    }

    private System.Collections.IEnumerator ApplyKnockback(Rigidbody2D rb, Vector2 direction)
    {
        var movementScript = rb.GetComponent<TopDownController>();
        if (movementScript != null)
            movementScript.LockInput(knockbackDuration);

        rb.linearVelocity = direction * knockbackForce;
        CameraShake.Instance.shakePreset = CameraShake.ShakePreset.Small;
        CameraShake.Instance.TriggerShake(direction);
        yield return new WaitForSeconds(knockbackDuration);
    }
}

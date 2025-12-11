using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(EnemyChaseLogic))]
public class JuggernautEnemy : MonoBehaviour
{
    [Header("Movement & Charge Settings")]
    public float chargeSpeed = 8f;
    public float chargeDuration = 0.75f;
    public float chargeCooldown = 3f;

    [Header("Knockback Settings")]
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.4f;

    [Header("Blink Warning Settings")]
    public float blinkDuration = 0.25f;
    public Color warningColor = new Color(0.5f, 0, 0); // Dark red

    private Rigidbody2D rb;
    private Transform player;
    private EnemyChaseLogic chaseLogic;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private bool isCharging = false;
    private bool canCharge = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        chaseLogic = GetComponent<EnemyChaseLogic>();
    }

    void Update()
    {
        if (player == null || chaseLogic == null || !chaseLogic.Aggroed || isCharging) return;

        Vector2 direction = (player.position - transform.position).normalized;

        if (canCharge)
        {
            StartCoroutine(ChargeAtPlayer(direction));
        }
    }

    IEnumerator ChargeAtPlayer(Vector2 direction)
    {
        isCharging = true;
        canCharge = false;

        // 🔴 Flash red before charging
        yield return StartCoroutine(BlinkWarning());

        // Disable normal chase logic
        if (chaseLogic != null)
            chaseLogic.enabled = false;

        rb.linearVelocity = direction * chargeSpeed;

        yield return new WaitForSeconds(chargeDuration);

        rb.linearVelocity = Vector2.zero;
        isCharging = false;

        // Re-enable chase logic
        if (chaseLogic != null)
            chaseLogic.enabled = true;

        yield return new WaitForSeconds(chargeCooldown);
        canCharge = true;
    }

    IEnumerator BlinkWarning()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = warningColor;
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isCharging) return;

        HeroStats playerStats = collision.gameObject.GetComponent<HeroStats>();
        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (playerStats != null && playerRb != null)
        {
            if (!playerStats.GodMode)
            {
                Vector2 knockDir = (collision.transform.position - transform.position).normalized;
                playerStats.Health -= 2;
                StartCoroutine(ApplyKnockback(playerRb, knockDir));
            }
        }
    }

    IEnumerator ApplyKnockback(Rigidbody2D targetRb, Vector2 direction)
    {
        TopDownController controller = targetRb.GetComponent<TopDownController>();
        if (controller != null)
            controller.LockInput(knockbackDuration);

        targetRb.linearVelocity = direction * knockbackForce;
        CameraShake.Instance.shakePreset = CameraShake.ShakePreset.Medium;
        CameraShake.Instance.TriggerShake(direction);


        yield return new WaitForSeconds(knockbackDuration);
    }
}

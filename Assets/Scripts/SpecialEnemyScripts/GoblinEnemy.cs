using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(EnemyChaseLogic))]
public class GoblinEnemy : MonoBehaviour
{
    [Header("Speed Scaling Settings")]
    public float normalSpeed = 2f;
    public float maxSpeedMultiplier = 1.5f;
    public float accelerationRate = 0.5f; // Units per second
    private float currentSpeedMultiplier = 1f;

    [Header("Knockback Settings")]
    public float knockbackForce = 3f;
    public float knockbackDuration = 0.2f;

    private Rigidbody2D rb;
    private EnemyChaseLogic chaseLogic;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        chaseLogic = GetComponent<EnemyChaseLogic>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null || chaseLogic == null)
            return;

        if (chaseLogic.Aggroed)
        {
            // Gradually increase speed
            if (currentSpeedMultiplier < maxSpeedMultiplier)
            {
                currentSpeedMultiplier += accelerationRate * Time.deltaTime;
                currentSpeedMultiplier = Mathf.Min(currentSpeedMultiplier, maxSpeedMultiplier);
            }

            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * (normalSpeed * currentSpeedMultiplier);
        }
        else
        {
            // Reset on lost aggro
            currentSpeedMultiplier = 1f;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HeroStats playerStats = collision.gameObject.GetComponent<HeroStats>();
        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (playerStats != null && playerRb != null)
        {
            Vector2 knockDir = (collision.transform.position - transform.position).normalized;

            if (!playerStats.GodMode)
                playerStats.Health -= 1;

            // Start knockback
            StartCoroutine(ApplyKnockback(playerRb, knockDir));
        }
    }

    IEnumerator ApplyKnockback(Rigidbody2D targetRb, Vector2 direction)
    {
        TopDownController controller = targetRb.GetComponent<TopDownController>();
        if (controller != null)
            controller.LockInput(knockbackDuration);

        targetRb.linearVelocity = direction * knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);
    }
}

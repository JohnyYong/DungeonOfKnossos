using System.Collections;
using UnityEngine;

public class GhostEnemy : MonoBehaviour
{
    public bool collided = false;
    private SpriteRenderer spriteRenderer;
    public float fadeTimer = 1.0f;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (collided) return;

        if (other.gameObject.GetComponent<HeroStats>() != null)
        {
            collided = true;
            HeroStats stats = other.gameObject.GetComponent<HeroStats>();
            if (stats.GodMode)
                return;

            stats.Health--;
            BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
            if (boxCollider2D != null)
            {
                boxCollider2D.enabled = false; 
            }
            GetComponent<EnemyChaseLogic>().enabled = false; //Stops chasing since it is dying
            StartCoroutine(FadeAndDie());
        }
    }

    IEnumerator FadeAndDie()
    {
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsed < fadeTimer)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTimer);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}

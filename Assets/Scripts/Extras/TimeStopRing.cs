using UnityEngine;

public class TimeStopRing : MonoBehaviour
{
    public float scaleSpeed = 5f;
    public float maxScale = 7f;
    public float minScale = 0.1f;

    private SpriteRenderer sr;
    private Color originalColor;
    private bool shrinking = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    void Update()
    {
        float delta = scaleSpeed * Time.unscaledDeltaTime;

        float currentScale = transform.localScale.x;
        float targetScale = shrinking ? minScale : maxScale;
        float newScale = Mathf.MoveTowards(currentScale, targetScale, delta);

        transform.localScale = new Vector3(newScale, newScale, 1f);

        // Fade alpha
        float alpha = Mathf.InverseLerp(minScale, maxScale, shrinking ? newScale : maxScale - newScale);
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

        // Auto-destroy only after shrink finishes
        if (shrinking && newScale <= minScale)
            Destroy(gameObject);
    }

    public void StartShrinking()
    {
        shrinking = true;
    }
}

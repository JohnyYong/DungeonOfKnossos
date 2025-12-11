using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class TemporalFairyCompanion : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;

    [Header("Time Slow Settings")]
    public float slowTimeScale = 0.2f;
    public float slowDuration = 3f;
    public GameObject circleEffectPrefab;
    public float circleMaxScale = 6f;
    public float circleAnimDuration = 0.5f;
    public Transform effectSpawnPoint;
    public int pulseCount = 3;
    public float delayBetweenPulses = 0.3f;

    private bool isSlowingTime = false;

    [Header("Orbit Settings")]
    public float orbitRadius = 1.5f;
    public float orbitSpeed = 180f; // degrees per second

    private float currentAngle = 0f;


    [Header("Camera Shake Settings")]
    public float shakeCooldown = 0.2f;
    public CameraShake.ShakePreset shakePreset = CameraShake.ShakePreset.Medium;

    private float shakeTimer = 0f;
    private bool shakingEnabled = false;

    [Header("GreyScale Efect")]
    private GreyscaleEffect greyscaleEffect;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        UiStatsDisplay.Instance.ShowFairyMessage("Press E to slow down time!");
        greyscaleEffect = Camera.main.GetComponent<GreyscaleEffect>();
    }

    void Update()
    {
        OrbitAroundPlayer();
        if (Input.GetKeyDown(KeyCode.E) && !isSlowingTime)
        {
            isSlowingTime = true;
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            StartCoroutine(ActivateTimeSlow());
        }

        if (shakingEnabled)
        {
            shakeTimer -= Time.unscaledDeltaTime;
            if (shakeTimer <= 0f)
            {
                CameraShake.Instance.shakePreset = shakePreset;
                CameraShake.Instance.TriggerShake();
                shakeTimer = shakeCooldown;
            }
        }
    }


    void OrbitAroundPlayer()
    {
        if (player == null) return;

        // Update orbit angle (degrees per second)
        currentAngle += orbitSpeed * Time.unscaledDeltaTime;
        if (currentAngle >= 360f)
            currentAngle -= 360f;

        // Calculate position
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
        transform.position = player.position + offset;

        Vector3 direction = (transform.position - player.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    public bool IsSlowingTime()
    {
        return isSlowingTime;
    }

    IEnumerator ActivateTimeSlow()
    {
        // Slow time
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        GameObject circleEffect = null;

        shakingEnabled = true;

        // Fade in grayscale
        StartCoroutine(FadeGreyscale(0f, 1f, 0.5f)); // fade in over 0.5s

        // Time pulses
        for (int i = 0; i < pulseCount; i++)
        {
             circleEffect = Instantiate(
                circleEffectPrefab,
                effectSpawnPoint ? effectSpawnPoint.position : player.position,
                Quaternion.identity
            );

            StartCoroutine(ScaleEffect(circleEffect.transform, Vector3.zero, Vector3.one * circleMaxScale, circleAnimDuration));
            yield return new WaitForSecondsRealtime(delayBetweenPulses);
        }

        shakingEnabled = false;


        // Wait
        yield return new WaitForSecondsRealtime(slowDuration);

        // Shrink the circle back
        if (circleEffect != null)
        {
            circleEffect.GetComponent<TimeStopRing>()?.StartShrinking();
        }

        // Fade out grayscale
        StartCoroutine(FadeGreyscale(1f, 0f, 0.5f));

        // Resume time
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        isSlowingTime = false;
    }
    IEnumerator ScaleEffect(Transform obj, Vector3 from, Vector3 to, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
 
            float t = Mathf.Clamp01(timer / duration);
            float eased = 1f - Mathf.Pow(1f - t, 2); // Ease-out curve
            obj.localScale = Vector3.Lerp(from, to, eased);

            yield return null;
        }

        obj.localScale = to;
    }

    IEnumerator FadeGreyscale(float from, float to, float duration)
    {
        if (greyscaleEffect == null) yield break;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float eased = 1f - Mathf.Pow(1f - t, 2); // ease-out
            greyscaleEffect.grayscaleIntensity = Mathf.Lerp(from, to, eased);
            yield return null;
        }

        greyscaleEffect.grayscaleIntensity = to;
    }

}

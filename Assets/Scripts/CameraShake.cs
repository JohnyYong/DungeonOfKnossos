// CameraShake.cs
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public enum ShakePreset { ExtraSmall, Small, Medium, Large }
    public static CameraShake Instance;
    void Awake() { Instance = this; }

    [Header("Shake Toggles")]
    public bool shakeX = true;
    public bool shakeY = true;
    public bool shakeRoll = false;

    [Header("Shake Settings")]
    public ShakePreset shakePreset = ShakePreset.Medium;
    public bool shakeEnabled = true;
    public float duration = 0.5f;
    public AnimationCurve fadeIn = AnimationCurve.EaseInOut(0, 0, 0.1f, 1);
    public AnimationCurve fadeOut = AnimationCurve.EaseInOut(0, 1, 0.9f, 0);

    private float timer = 0f;
    private float totalDuration;
    private Vector3 shakeOffset = Vector3.zero;
    private float shakeRotation = 0f;
    private bool shaking = false;
    private Vector2 directionalBias = Vector2.zero;
    private bool useDirectionalShake = false;
    private bool continuousShake = false;
    private float continuousIntensity = 1f;

    private float GetAmplitude()
    {
        switch (shakePreset)
        {
            case ShakePreset.ExtraSmall: return 0.2f;
            case ShakePreset.Small: return 0.3f;
            case ShakePreset.Medium: return 0.5f;
            case ShakePreset.Large: return 0.9f;
            default: return 0.2f;
        }
    }

    public void TriggerShake(float customDuration = -1f)
    {
        if (!shakeEnabled) return;
        continuousShake = false;
        totalDuration = (customDuration > 0f) ? customDuration : duration;
        timer = 0f;
        shaking = true;
    }

    public void TriggerContinuousShake(float intensity = 1f)
    {
        if (!shakeEnabled) return;
        continuousShake = true;
        continuousIntensity = intensity;
        shaking = true;
    }

    public void StopShake()
    {
        shaking = false;
        shakeOffset = Vector3.zero;
        shakeRotation = 0f;
    }

    public void TriggerShake(Vector2 direction, float customDuration = -1f)
    {
        useDirectionalShake = true;
        directionalBias = direction.normalized;
        TriggerShake(customDuration);
    }

    void Update()
    {
        if (!shaking) return;

        if (continuousShake)
        {
            // Continuous shaking (for sword rise)
            float amplitude = GetAmplitude() * continuousIntensity;

            float shakeXOffset = shakeX ? (Mathf.PerlinNoise(Time.time * 30f, 0f) - 0.5f) * amplitude : 0f;
            float shakeYOffset = shakeY ? (Mathf.PerlinNoise(0f, Time.time * 30f) - 0.5f) * amplitude : 0f;
            float rollOffset = shakeRoll ? (Mathf.PerlinNoise(Time.time * 10f, Time.time * 10f) - 0.5f) * amplitude * 30f : 0f;

            shakeOffset = new Vector3(shakeXOffset, shakeYOffset, 0f);
            shakeRotation = rollOffset;
        }
        else
        {
            // Timed shaking (for other effects)
            timer += Time.unscaledDeltaTime;
            float normalizedTime = timer / totalDuration;
            float intensity = 0f;

            if (normalizedTime < 1f)
            {
                if (normalizedTime < 0.5f)
                    intensity = fadeIn.Evaluate(normalizedTime * 2f);
                else
                    intensity = fadeOut.Evaluate((normalizedTime - 0.5f) * 2f);

                float amplitude = GetAmplitude() * intensity;
                float shakeXOffset = 0f;
                float shakeYOffset = 0f;

                if (useDirectionalShake)
                {
                    Vector2 offset = directionalBias * amplitude * (Mathf.PerlinNoise(Time.time * 50f, 0f) - 0.5f) * 2f;
                    shakeXOffset = shakeX ? offset.x : 0f;
                    shakeYOffset = shakeY ? offset.y : 0f;
                }
                else
                {
                    shakeXOffset = shakeX ? (Mathf.PerlinNoise(Time.time * 30f, 0f) - 0.5f) * amplitude : 0f;
                    shakeYOffset = shakeY ? (Mathf.PerlinNoise(0f, Time.time * 30f) - 0.5f) * amplitude : 0f;
                }

                float rollOffset = shakeRoll ? (Mathf.PerlinNoise(Time.time * 10f, Time.time * 10f) - 0.5f) * amplitude * 30f : 0f;

                shakeOffset = new Vector3(shakeXOffset, shakeYOffset, 0f);
                shakeRotation = rollOffset;
            }
            else
            {
                // Reset after shaking ends
                shakeOffset = Vector3.zero;
                shakeRotation = 0f;
                shaking = false;
                useDirectionalShake = false;
            }
        }
    }

    public Vector3 GetShakeOffset()
    {
        return shakeOffset;
    }

    public float GetShakeRotation()
    {
        return shakeRotation;
    }
}
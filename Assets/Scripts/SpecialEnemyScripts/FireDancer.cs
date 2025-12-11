using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDancer : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform waypointsParent;
    private Transform centerPoint, leftPoint, rightPoint;
    private Transform topPoint, bottomPoint;

    [Header("Movement Settings")]
    public float moveDuration = 1.5f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private Coroutine movementRoutine;

    [Header("Attack Timings")]
    public float baseSpiralCooldown = 3f;
    public float minSpiralCooldown = 1f;
    public float cooldownDecreasePerAttack = 0.2f;
    public int attacksBeforeReset = 5;
    private float currentSpiralCooldown;
    private int attackCount = 0;
    public float fireCircleCooldown = 10f;

    [Header("Spiral Ignite Attack")]
    public GameObject flameBurstPrefab;
    public int spiralSteps = 12;
    public float spiralDelay = 0.1f;
    public float spiralRadiusStart = 1f;
    public float spiralRadiusEnd = 3f;
    public float spiralHeightVariation = 0.5f;
    public AnimationCurve spiralSizeCurve = AnimationCurve.EaseInOut(0, 0.5f, 1, 1.5f);
    public AnimationCurve spiralAlphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float spiralRotationSpeed = 180f;
    public float spiralLifetime = 2f;
    public int maxConcurrentFlames = 20;
    private int activeFlames = 0;

    [Header("Fire Circle Attack")]
    public GameObject fireCirclePrefab;
    public int fireCircleCount = 8;
    public float fireCircleRadius = 2f;
    public float fireCircleBuildTime = 1.5f;
    public float fireCircleSpinSpeed = 90f;
    public float fireCircleDuration = 10f;

    [Header("Effects")]
    public ParticleSystem speedUpEffect;
    public Light attackLight;
    public float maxLightIntensity = 5f;
    public AudioClip igniteSound;
    public float igniteSoundVolume = 0.7f;

    private float spiralTimer = 0f;
    private float fireCircleTimer = 0f;
    private List<GameObject> orbitingFires = new List<GameObject>();
    private float[] fireAngles;
    private bool isSpawningFireCircle = false;
    private bool stopImmediately = false;
    private Coroutine fireCircleCoroutine;
    private EnemyChaseLogic enemyChaseLogic;

    private Coroutine fireCircleShakeRoutine;

    private bool fireCircleFullySpawned = false;

    void Start()
    {
        enemyChaseLogic = GetComponent<EnemyChaseLogic>();
        currentSpiralCooldown = baseSpiralCooldown;

        if (waypointsParent != null)
        {
            centerPoint = waypointsParent.Find("Centrepoint");
            leftPoint = waypointsParent.Find("Leftpoint");
            rightPoint = waypointsParent.Find("Rightpoint");
            topPoint = waypointsParent.Find("Toppoint");
            bottomPoint = waypointsParent.Find("Bottompoint");

            if (topPoint == null || bottomPoint == null)
            {
                Debug.LogError("[FireDancer] Top or Bottom waypoints missing!");
                enabled = false;
                return;
            }

            if (centerPoint == null || leftPoint == null || rightPoint == null || topPoint == null || bottomPoint == null)
            {
                Debug.LogError("[FireDancer] Some waypoints are missing!");
                enabled = false;
                return;
            }


            movementRoutine = StartCoroutine(InfinityMovement());
        }
        else
        {
            Debug.LogError("[FireDancer] Waypoints parent not assigned!");
            enabled = false;
        }
    }

    void Update()
    {
        if (!enemyChaseLogic.Aggroed)
        {
            return;
        }
        // Allow spiral to always update
        spiralTimer += Time.deltaTime;

        if (spiralTimer >= currentSpiralCooldown)
        {
            StartCoroutine(SpiralIgnite());
            spiralTimer = 0f;
            attackCount++;

            if (attackCount < attacksBeforeReset)
            {
                currentSpiralCooldown = Mathf.Max(
                    minSpiralCooldown,
                    currentSpiralCooldown - cooldownDecreasePerAttack
                );
            }
            else
            {
                currentSpiralCooldown = baseSpiralCooldown;
                attackCount = 0;
            }
        }

        // Fire circle should only trigger if not already playing
        if (!isSpawningFireCircle)
        {
            fireCircleTimer += Time.deltaTime;

            if (fireCircleTimer >= fireCircleCooldown)
            {
                BeginFireCircleAttack();
                fireCircleTimer = 0f;
            }
        }

        UpdateOrbitingFires();
    }


    void UpdateOrbitingFires()
    {
        if (!fireCircleFullySpawned)
            return;

        for (int i = 0; i < orbitingFires.Count; i++)
        {
            if (orbitingFires[i] == null) continue;

            fireAngles[i] += fireCircleSpinSpeed * Time.deltaTime;
            float rad = fireAngles[i] * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * fireCircleRadius;
            orbitingFires[i].transform.position = transform.position + offset;
        }
    }


    IEnumerator InfinityMovement()
    {
        Transform[] pattern = new Transform[] {
        leftPoint, topPoint, rightPoint, bottomPoint, centerPoint
    };

        while (true)
        {
            foreach (Transform point in pattern)
            {
                yield return StartCoroutine(CurveTo(point));
            }
        }
    }



    IEnumerator CurveTo(Transform target)
    {
        Vector3 start = transform.position;
        Vector3 end = target.position;
        float t = 0f;

        while (t < moveDuration)
        {
            if (stopImmediately) yield break;

            float progress = curve.Evaluate(t / moveDuration);
            transform.position = Vector3.Lerp(start, end, progress);
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
    }

    IEnumerator SpiralIgnite()
    {
        if (igniteSound != null)
            AudioSource.PlayClipAtPoint(igniteSound, transform.position, igniteSoundVolume);

        if (speedUpEffect != null)
            speedUpEffect.Play();

        if (attackLight != null)
        {
            float intensity = Mathf.Lerp(1f, maxLightIntensity,
                1 - (currentSpiralCooldown - minSpiralCooldown) / (baseSpiralCooldown - minSpiralCooldown));
            attackLight.intensity = intensity;
        }

        float currentAngle = 0f;
        float rotationDirection = Random.value > 0.5f ? 1f : -1f;

        for (int i = 0; i < spiralSteps; i++)
        {
            if (activeFlames >= maxConcurrentFlames)
                yield return new WaitUntil(() => activeFlames < maxConcurrentFlames);

            float spiralProgress = (float)i / spiralSteps;
            float currentRadius = Mathf.Lerp(spiralRadiusStart, spiralRadiusEnd, spiralProgress);
            float heightOffset = Mathf.Sin(spiralProgress * Mathf.PI) * spiralHeightVariation;

            Vector3 offset = Quaternion.Euler(0, 0, currentAngle) * Vector3.right * currentRadius;
            offset.y += heightOffset;
            Vector3 flamePos = transform.position + offset;

            GameObject flame = Instantiate(flameBurstPrefab, flamePos, Quaternion.identity);
            activeFlames++;

            FlameBehavior fb = flame.AddComponent<FlameBehavior>();
            fb.onDestroyedCallback += () => activeFlames--;

            float sizeMultiplier = spiralSizeCurve.Evaluate(spiralProgress);
            flame.transform.localScale *= sizeMultiplier;

            SpriteRenderer sr = flame.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = spiralAlphaCurve.Evaluate(spiralProgress);
                sr.color = c;
            }

            StartCoroutine(RotateFlame(flame.transform, rotationDirection));
            Destroy(flame, spiralLifetime);

            currentAngle += (360f / (spiralSteps / 2f)) * rotationDirection;
            yield return new WaitForSeconds(spiralDelay);
        }
    }

    IEnumerator RotateFlame(Transform flame, float direction)
    {
        float rotationSpeed = spiralRotationSpeed * direction;
        float elapsed = 0f;

        while (flame != null && elapsed < spiralLifetime)
        {
            flame.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void BeginFireCircleAttack()
    {
        isSpawningFireCircle = true;
        stopImmediately = true;

        if (movementRoutine != null)
            StopCoroutine(movementRoutine);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // 🔥 Start continuous shake
        if (fireCircleShakeRoutine != null)
            StopCoroutine(fireCircleShakeRoutine);
        fireCircleShakeRoutine = StartCoroutine(ContinuousShakeDuringFireCircle());

        fireCircleCoroutine = StartCoroutine(FireCircleAttack());
    }

    IEnumerator FireCircleAttack()
    {
        orbitingFires.Clear();
        fireAngles = new float[fireCircleCount];
        float angleStep = 360f / fireCircleCount;
        fireCircleFullySpawned = false;

        if (fireCircleShakeRoutine != null)
            StopCoroutine(fireCircleShakeRoutine);
        fireCircleShakeRoutine = StartCoroutine(ContinuousShakeDuringFireCircle());

        Vector3 circleOrigin = transform.position;

        for (int i = 0; i < fireCircleCount; i++)
        {
            float angle = angleStep * i;
            fireAngles[i] = angle;

            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * fireCircleRadius;
            Vector3 spawnPos = circleOrigin + offset;

            GameObject flame = Instantiate(fireCirclePrefab, spawnPos, Quaternion.identity);
            orbitingFires.Add(flame);
            StartCoroutine(ScaleFire(flame.transform, 1f)); // or 0.5f, etc.

            yield return new WaitForSeconds(fireCircleBuildTime / fireCircleCount);
        }
        fireCircleFullySpawned = true;


        // Stop shake right after all fires spawned
        if (fireCircleShakeRoutine != null)
        {
            StopCoroutine(fireCircleShakeRoutine);
            fireCircleShakeRoutine = null;
        }

        stopImmediately = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.isKinematic = false;

        movementRoutine = StartCoroutine(InfinityMovement());
        yield return new WaitForSeconds(fireCircleDuration);
        yield return StartCoroutine(FadeOutFires());

        orbitingFires.Clear();
        isSpawningFireCircle = false;
    }

    IEnumerator ContinuousShakeDuringFireCircle()
    {
        while (isSpawningFireCircle)
        {
            CameraShake.Instance.shakePreset = CameraShake.ShakePreset.Small;
            CameraShake.Instance.TriggerShake(0.15f); // very short shake
            yield return new WaitForSeconds(0.1f);    // slight overlap is okay
        }
    }


    IEnumerator ScaleFire(Transform fireTransform, float duration)
    {
        Vector3 originalScale = fireTransform.localScale;
        fireTransform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            fireTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        fireTransform.localScale = originalScale;
    }

    IEnumerator FadeOutFires()
    {
        float fadeDuration = 0.5f;
        float elapsed = 0f;
        List<float> initialAlphas = new List<float>();
        List<SpriteRenderer> renderers = new List<SpriteRenderer>();

        foreach (var flame in orbitingFires)
        {
            if (flame != null)
            {
                var sr = flame.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    renderers.Add(sr);
                    initialAlphas.Add(sr.color.a);
                }
            }
        }

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                {
                    Color newColor = renderers[i].color;
                    newColor.a = Mathf.Lerp(initialAlphas[i], 0f, t);
                    renderers[i].color = newColor;
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var flame in orbitingFires)
        {
            if (flame != null)
                Destroy(flame);
        }
    }

    void OnDestroy()
    {
        StopAllCoroutines(); // prevent continued access to destroyed Transforms

        foreach (var flame in orbitingFires)
        {
            if (flame != null)
                Destroy(flame);
        }
    }

}

public class FlameBehavior : MonoBehaviour
{
    public System.Action onDestroyedCallback;

    private void OnDestroy()
    {
        if (onDestroyedCallback != null)
            onDestroyedCallback();
    }
}
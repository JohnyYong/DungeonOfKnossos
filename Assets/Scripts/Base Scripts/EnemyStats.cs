/*******************************************************************************
File:      EnemyStats.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component controls all behaviors for enemies in the game.

*******************************************************************************/
using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour
{
    public GameObject EnemyHealthBarPrefab;
    private GameObject HealthBar;
    private HealthBar HealthBarComp;
    public GameObject purpleKey;

    [Header("Death Animation")]
    public GameObject deathParticleEffect;   // Drag your VFX prefab here
    private SpriteRenderer spriteRenderer;
    private bool isDying = false;

    private GameObject player;

    public int StartingHealth = 3;
    public int Health
    {
        get { return _Health; }

        set
        {
            HealthBarComp.Health = value;
            _Health = value;
        }

    }
    private int _Health;


    // Start is called before the first frame update
    void Start()
    {
        //Initialize enemy health bar
        HealthBar = Instantiate(EnemyHealthBarPrefab);
        HealthBar.GetComponent<ObjectFollow>().ObjectToFollow = transform;
        HealthBarComp = HealthBar.GetComponent<HealthBar>();
        HealthBarComp.MaxHealth = StartingHealth;
        HealthBarComp.Health = StartingHealth;
        Health = StartingHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0 && !isDying)
        {
            isDying = true;

            if (gameObject.name.Contains("Boss"))
            {
                StartCoroutine(BossDeathSequence(gameObject.name));
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var bullet = col.GetComponent<BulletLogic>();
        if (bullet != null && bullet.Team == Teams.Player)
        {
            if (player.GetComponent<HeroStats>().GodMode)
            {
                Health -= 999;

            }
            else
            {
                Health -= bullet.Power;
            }

            //if (Health <= 0 && !isDying)
            //{
            //    isDying = true;

            //    if (gameObject.name.Contains("Boss"))
            //    {
            //        StartCoroutine(BossDeathSequence(gameObject.name));
            //    }
            //    else
            //    {
            //        Destroy(gameObject);
            //    }
            //}

        }
    }
    IEnumerator BossDeathSequence(string bossName)
    {
        if (bossName.Contains("FireDancer"))
        {
            // Stop FireDancer logic
            FireDancer dancer = GetComponent<FireDancer>();
            if (dancer != null)
            {
                dancer.StopAllCoroutines(); // Stop any ongoing attacks/movement
                dancer.enabled = false;
            }
        }
        else if (bossName.Contains("Reaper"))
        {
            var shooter = GetComponent<BossSpiralScytheShooter>();
            if (shooter != null) shooter.enabled = false;

            // Stop teleporting
            var teleport = GetComponent<ReaperTeleport>();
            if (teleport != null) teleport.enabled = false;
        }

        // Stop chase
        EnemyChaseLogic chase = GetComponent<EnemyChaseLogic>();
        if (chase != null)
            chase.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Spawn death VFX
        if (deathParticleEffect != null)
            Instantiate(deathParticleEffect, transform.position, Quaternion.identity);

        // Fade out sprite
        float duration = 2f;
        StartCoroutine(ContinuousDeathShake(duration));
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        // Drop key
        if (purpleKey != null)
            Instantiate(purpleKey, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    IEnumerator ContinuousDeathShake(float duration, float interval = 0.15f)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            CameraShake.Instance.shakePreset = CameraShake.ShakePreset.Large;
            CameraShake.Instance.TriggerShake();
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }


    private void OnDestroy()
    {
        DestroyImmediate(HealthBar);
    }
}

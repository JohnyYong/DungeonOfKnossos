using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BlackHoleBoss : MonoBehaviour
{
    public Transform blackHoleBody; // assign black hole visual
    public Transform eyeCenter;     // center of eye (where projectiles spawn)
    public GameObject[] projectilePrefabs;
    public float projectileSpawnInterval = 1.5f;
    public float suctionForce = 10f;
    public float suctionRadius = 6f;

    private Transform player;
    private EnemyChaseLogic enemyChaseLogic;
    public bool isShaking = false;

    void Start()
    {
        blackHoleBody = GameObject.FindGameObjectWithTag("BlackholeBody").transform;
        eyeCenter = gameObject.transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemyChaseLogic = GetComponent<EnemyChaseLogic>();
        StartCoroutine(SpawnProjectiles());

    }

    void Update()
    {
        if (!enemyChaseLogic.Aggroed)
        {
            return;
        }

        SpinBlackHole();
        ApplySuction();
    }

    void SpinBlackHole()
    {
        blackHoleBody.Rotate(0, 0, 120f * Time.deltaTime);
    }
    void ApplySuction()
    {
        if (player == null) return;

        Vector2 direction = transform.position - player.position;
        float distance = direction.magnitude;

        if (distance < suctionRadius)
        {
            // Start camera shake only if not already shaking
            if (!isShaking)
            {
                CameraShake.Instance.shakePreset = CameraShake.ShakePreset.ExtraSmall;
                CameraShake.Instance.TriggerContinuousShake();
                isShaking = true;
            }

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            rb.AddForce(direction.normalized * suctionForce * Time.deltaTime, ForceMode2D.Force);

            if (distance < 0.1f)
            {
                player.GetComponent<HeroStats>().Health -= player.GetComponent<HeroStats>().MaxHealth;
                Debug.Log("Player sucked in!");
            }
        }
        else
        {
            // Stop shake when player exits suction zone
            if (isShaking)
            {
                CameraShake.Instance.StopShake();
                isShaking = false;
            }
        }
    }


    IEnumerator SpawnProjectiles()
    {
        while (true)
        {
            yield return new WaitForSeconds(projectileSpawnInterval);
            GameObject proj = Instantiate(projectilePrefabs[Random.Range(0, projectilePrefabs.Length)], eyeCenter.position, Quaternion.identity);
            proj.GetComponent<BlackHoleProjectile>()?.Initialize(transform.position);
        }
    }
}

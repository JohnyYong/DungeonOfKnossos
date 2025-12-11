using UnityEngine;
using System.Collections;

public class BossSpiralScytheShooter : MonoBehaviour
{
    public GameObject ScytheBulletPrefab;
    public float ShootCooldown = 1f;
    public float BulletSpeed = 8f;
    public int Power = 1;

    [Header("Spiral Settings")]
    public int BulletsPerCycle = 12;
    public float SpiralRotationSpeed = 20f;

    private float timer = 0f;
    private float spiralRotation = 0f;
    private EnemyChaseLogic enemyChaseLogic;
    private int shotsFired = 0;
    private ReaperTeleport teleport;

    void Start()
    {
        enemyChaseLogic = GetComponent<EnemyChaseLogic>();
        teleport = GetComponent<ReaperTeleport>();
    }
    void Update()
    {
        if (enemyChaseLogic.Aggroed && (teleport == null || !teleport.isTeleporting))
        {
            timer += Time.deltaTime;

            if (timer >= ShootCooldown)
            {
                FireSpiralScythes();
                timer = 0f;
            }
        }
    }

    void FireSpiralScythes()
    {
        for (int i = 0; i < BulletsPerCycle; i++)
        {
            float angle = (360f / BulletsPerCycle) * i + spiralRotation;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;

            GameObject bullet = Instantiate(ScytheBulletPrefab, transform.position, Quaternion.identity);
            bullet.transform.up = direction;

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = direction * BulletSpeed;

            BulletLogic logic = bullet.GetComponent<BulletLogic>();
            if (logic != null)
            {
                logic.Power = Power;
                logic.lifetime = 1f;
                logic.spinVisual = true;
            }
        }

        CameraShake.Instance.shakePreset = CameraShake.ShakePreset.Medium;
        CameraShake.Instance.TriggerShake(); CameraShake.Instance.TriggerShake();

        spiralRotation += SpiralRotationSpeed;
        shotsFired++;

        if (shotsFired >= 3)
        {
            if (teleport != null)
                teleport.TeleportToRandomWaypoint();
            shotsFired = 0; // reset counter
            Debug.Log("Telporting");
        }
    }

}

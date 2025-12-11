#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif
using UnityEngine;

[RequireComponent(typeof(EnemyChaseLogic))]
public class EnemyShootLogic : MonoBehaviour
{
    public enum ShotType { Single, Shotgun, Burst, Spiral }

    public GameObject BulletPrefab;
    public ShotType Type = ShotType.Single;
    public int Power = 1;
    public float ShootCooldown = 1f;
    public float BulletSpeed = 8f;

    // Shotgun only
    public int ShotgunBullets = 3;
    public float ShotgunAngle = 0.5f;

    // Burst only
    public int BurstCount = 3;
    public float BurstInterval = 0.1f;

    // Spiral only
    public int SpiralBulletsPerCycle = 8;
    private float spiralRotation = 0f;

    private EnemyChaseLogic ChaseBehavior;
    private float Timer = 0f;
    private float burstTimer = 0f;
    private int burstShotsRemaining = 0;

    void Start()
    {
        ChaseBehavior = GetComponent<EnemyChaseLogic>();
    }

    void Update()
    {
        if (!ChaseBehavior.Aggroed)
            return;

        Timer += Time.deltaTime;

        if (Type == ShotType.Burst && burstShotsRemaining > 0)
        {
            burstTimer += Time.deltaTime;
            if (burstTimer >= BurstInterval)
            {
                FireBullet(transform.up);
                burstShotsRemaining--;
                burstTimer = 0f;
            }
            return;
        }

        if (Timer >= ShootCooldown)
        {
            switch (Type)
            {
                case ShotType.Single:
                    FireBullet(transform.up);
                    break;

                case ShotType.Shotgun:
                    for (int i = 0; i < ShotgunBullets; i++)
                    {
                        float angleOffset = -ShotgunAngle * ((float)(ShotgunBullets - 1) / 2f) + ShotgunAngle * i;
                        Vector2 direction = RotateVector(transform.up, angleOffset);
                        FireBullet(direction);
                    }
                    break;

                case ShotType.Burst:
                    burstShotsRemaining = BurstCount;
                    burstTimer = BurstInterval; // Fire immediately
                    break;

                case ShotType.Spiral:
                    for (int i = 0; i < SpiralBulletsPerCycle; i++)
                    {
                        float angle = (360f / SpiralBulletsPerCycle) * i + spiralRotation;
                        Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
                        FireBullet(dir);
                    }
                    spiralRotation += 15f; // Rotate spiral slightly each cycle
                    break;
            }

            Timer = 0f;
        }

    }

    void FireBullet(Vector2 direction)
    {
        var bullet = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
        bullet.transform.up = direction.normalized;
        bullet.GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * BulletSpeed;
        bullet.GetComponent<BulletLogic>().Power = Power;
    }

    Vector2 RotateVector(Vector2 vec, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            cos * vec.x - sin * vec.y,
            sin * vec.x + cos * vec.y
        );
    }
}

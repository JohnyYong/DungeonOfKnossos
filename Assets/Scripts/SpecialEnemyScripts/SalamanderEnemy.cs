using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SalamanderEnemy : MonoBehaviour
{
    public GameObject fireballPrefab;
    private bool firingFireBall;
    private EnemyChaseLogic enemyChaseLogic;
    public float numFireBalls = 6;
    public float fireBallSpeed = 10f;
    public float fireballsPerCycle = 3;
    public float fireballCD = 5f;
    public float fireballTimer = 0f;
    private List<GameObject> fireShotout = new List<GameObject>();

    [Header("Fire Breath Settings")]
    public float breathDuration = 2f; // Duration of the breath attack
    public int flamesPerSecond = 10; // Number of flames per second
    public float flameSpreadAngle = 60f; // Degrees of cone
    public Transform fireMouth; // Optional: Where fire emits from
    private float breathTimer = 0f;
    private float flameTimer = 0f;
    private bool isBreathing = false;

    // Start is called before the first frame update
    void Start()
    {
        enemyChaseLogic = GetComponent<EnemyChaseLogic>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyChaseLogic.Aggroed)
        {
            if (!isBreathing)
            {
                fireballTimer += Time.deltaTime;

                if (fireballTimer >= fireballCD)
                {
                    isBreathing = true;
                    enemyChaseLogic.enabled = false;
                    fireballTimer = 0f;
                    breathTimer = 0f;
                    flameTimer = 0f;
                }
            }
            else
            {
                breathTimer += Time.deltaTime;
                flameTimer += Time.deltaTime;

                if (flameTimer >= 1f / flamesPerSecond)
                {
                    FlameBurst(); // Emit flame
                    flameTimer = 0f;
                }

                if (breathTimer >= breathDuration)
                {
                    isBreathing = false;
                    enemyChaseLogic.enabled = true;
                }
            }
        }
    }

    void FlameBurst()
    {
        int flameCount = 5;
        float angleStep = flameSpreadAngle / (flameCount - 1);
        float startAngle = -flameSpreadAngle / 2f;

        for (int i = 0; i < flameCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * transform.up;

            Vector3 spawnPos = fireMouth != null ? fireMouth.position : transform.position;
            GameObject flame = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
            flame.transform.up = dir;

            Rigidbody2D rb = flame.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = dir * fireBallSpeed;

            BulletLogic logic = flame.GetComponent<BulletLogic>();
            if (logic != null)
            {
                logic.Power = 1; // Slightly weaker per flame
                logic.spinVisual = false; // Optional: disable spin for cone effect
            }

            fireShotout.Add(flame);
        }
    }

}

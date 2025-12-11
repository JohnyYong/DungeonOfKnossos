using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyChaseLogic))]
public class MindDisrupterEnemy : MonoBehaviour
{
    public GameObject psychicBeamPrefab;
    public float shootCooldown = 4f;
    public float beamSpeed = 10f;

    private float shootTimer = 0f;
    private Transform player;
    private EnemyChaseLogic chaseLogic;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        chaseLogic = GetComponent<EnemyChaseLogic>();
    }

    void Update()
    {
        if (player == null || chaseLogic == null || !chaseLogic.Aggroed)
            return;

        shootTimer += Time.deltaTime;

        if (shootTimer >= shootCooldown)
        {
            FireBeam();
            shootTimer = 0f;
        }
    }

    void FireBeam()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        GameObject beam = Instantiate(psychicBeamPrefab, transform.position, Quaternion.identity);
        beam.transform.up = dir;

        Rigidbody2D rb = beam.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = dir * beamSpeed;
    }
}

/*******************************************************************************
File:      HeroShoot.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component is added to the player and is responsible for the player's
    shoot ability and rotating the player to face the mouse position.

*******************************************************************************/
using UnityEngine;

[RequireComponent(typeof(HeroStats))]

public class HeroShoot : MonoBehaviour
{
    public GameObject BulletPrefab;
    public float BulletSpeed = 5.0f;
    public float ShotCooldown = 1.0f;
    public int BulletCount = 1;          
    public int MaxBulletCount = 4;        
    public float SpreadAngle = 30f;
    public float angleOffsetGlobal = 10.0f;
    private float Timer = 1.0f;

    void Update()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;

        Vector2 shootDirection = (mouseWorld - transform.position).normalized;
        transform.up = shootDirection; // for visual rotation only

        Timer += Time.deltaTime;

        if (Timer >= ShotCooldown && Input.GetMouseButton(0))
        {
            ShootSpread(shootDirection); // pass the direction once
            Timer = 0f;
        }
    }


    void ShootSpread(Vector2 baseDirection)
    {
        float angleStep = (BulletCount == 1) ? 0 : SpreadAngle / (BulletCount - 1);
        float baseAngle = -SpreadAngle / 2 + angleOffsetGlobal;

        Vector3 spawnPos = transform.position;
        Vector2 playerVelocity = GetComponent<Rigidbody2D>() != null ? GetComponent<Rigidbody2D>().linearVelocity : Vector2.zero;

        for (int i = 0; i < BulletCount; i++)
        {
            float angle = baseAngle + i * angleStep;
            Quaternion spreadRot = Quaternion.Euler(0, 0, angle);
            Vector2 shootDir = spreadRot * baseDirection;

            GameObject bullet = Instantiate(BulletPrefab, spawnPos, Quaternion.LookRotation(Vector3.forward, shootDir));

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = shootDir * BulletSpeed + playerVelocity * 0.3f; // Optional: inherit 30% of player's speed
            }

            BulletLogic logic = bullet.GetComponent<BulletLogic>();
            if (logic != null)
            {
                logic.Power = GetComponent<HeroStats>().Power;
            }
        }
    }


    //for pickup to safely increase bullet count
    public void IncreaseBulletCount(int amount)
    {
        BulletCount = Mathf.Clamp(BulletCount + amount, 1, MaxBulletCount);
    }
}
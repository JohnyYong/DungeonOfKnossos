/*******************************************************************************
File:      EnemyChaseLogic.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/6/2018
Course:    CS186
Section:   Z

Description:
    This component is responsible for the chase behavior on some enemies.

*******************************************************************************/
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaseLogic : MonoBehaviour
{
    public float AggroRange = 8f;
    public float DeaggroRange = 10f;
    public float MoveSpeed = 5f;
    public bool Aggroed = false;

    private Transform Player;
    private Rigidbody2D RB;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("Hero").transform;
        RB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //No reference to player, Nothing to chase
        if (Player == null || !Player.gameObject.activeSelf)
        {
            RB.linearVelocity = Vector2.zero;
            Aggroed = false;
            return;
        }

        //If player is within aggro range, chase it!
        var dir = (Player.position - transform.position);
        if (dir.magnitude <= AggroRange)
        {
            //Rotate to face the player
            transform.up = dir.normalized;

            Aggroed = true;

            //Move at designated velocity
            RB.linearVelocity = transform.up * MoveSpeed;
        }

        if(dir.magnitude >= DeaggroRange)
        {
            Aggroed = false;
            RB.linearVelocity = Vector2.zero;
        }

    }
}

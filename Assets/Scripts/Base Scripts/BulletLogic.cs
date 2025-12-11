/*******************************************************************************
File:      BulletLogic.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component is added to the bullet and controls all of its behavior,
    including how to handle when different objects are hit.

*******************************************************************************/
using UnityEngine;

public enum Teams { Player, Enemy }

public class BulletLogic : MonoBehaviour
{
    public Teams Team = Teams.Player;
    public int Power = 1;
    [Header("Unique To Reaper")]
    public bool spinVisual = false;
    public float rotationSpeed = 360f;
    public float lifetime = 0.4f;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.isTrigger || col.tag == Team.ToString())
            return;

        Destroy(gameObject);
    }

    private void Update()
    {

        if (spinVisual)
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        if (lifetime <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            lifetime -= Time.deltaTime;
        }

    }

}

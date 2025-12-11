/*******************************************************************************
File:      DoorLogic.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This handles the behavior of Door objects that are destroyed when colliding
    with a player possessing the corresponding key.

*******************************************************************************/
using UnityEngine;
using UnityEngine.Rendering;

public enum DoorType { Silver, Gold, Purple }

public class DoorLogic : MonoBehaviour
{
    public DoorType Type = DoorType.Silver;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Check for collisions with player
        var hero = collision.gameObject.GetComponent<HeroStats>();
        if (hero != null)
        {
            if (Type == DoorType.Silver && hero.SilverKeys > 0)
            {
                hero.SilverKeys--;
                Destroy(gameObject);
            }

            if (Type == DoorType.Gold && hero.GoldKeys > 0)
            {
                hero.GoldKeys--;
                Destroy(gameObject);
            }

            if (Type == DoorType.Purple && hero.PurpleKeys > 0)
            {
                hero.PurpleKeys--;
                Destroy(gameObject);
            }
        }
    }
}

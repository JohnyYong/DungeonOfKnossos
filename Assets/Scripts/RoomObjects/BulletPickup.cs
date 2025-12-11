using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BulletPickup : MonoBehaviour
{
    public int increaseAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        HeroShoot shooter = other.GetComponent<HeroShoot>();
        if (shooter != null)
        {
            shooter.IncreaseBulletCount(increaseAmount);

            Destroy(gameObject);
        }
    }
}

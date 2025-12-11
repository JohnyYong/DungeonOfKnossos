using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class MindBeamBullet : MonoBehaviour
{
    public float invertDuration = 3f;
    public float speed = 5f;
    public float rotationSpeed = 100.0f;
    private void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = transform.up * speed;
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player"))
            return;

        if (col.GetComponent<HeroStats>() != null)
        {
            if (col.GetComponent<HeroStats>().GodMode)
            {
                return;
            }
        }
        TopDownController controller = col.GetComponent<TopDownController>();
        if (controller != null)
        {
            controller.InvertControls(invertDuration);
        }

        Destroy(gameObject);
    }
}

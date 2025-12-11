using UnityEngine;
using System.Collections;

public class Web : MonoBehaviour
{
    public float slowDuration = 2f;
    public float slowFactor = 0.25f; // 25% of normal speed

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<HeroStats>() != null)
        {
            if (other.GetComponent<HeroStats>().GodMode)
                return;
        }

        TopDownController controller = other.GetComponent<TopDownController>();
        if (controller != null)
        {
            StartCoroutine(SlowPlayer(controller));
        }
    }

    private IEnumerator SlowPlayer(TopDownController controller)
    {
        float originalSpeed = controller.Speed;
        controller.Speed *= slowFactor;

        yield return new WaitForSeconds(slowDuration);

        controller.Speed = originalSpeed;
    }
}

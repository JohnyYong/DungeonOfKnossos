using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class BlackHoleProjectile : MonoBehaviour
{
    public float speed = 3f;                  // Optional forward push
    public float rotateSpeed = 180f;          // Orbit rotation speed
    public float initialOrbitRadius = 0.5f;   // Starting radius
    public float orbitExpandSpeed = 0.5f;     // How fast it expands outward
    public float lifeTime = 2f;

    private float orbitRadius;
    private Vector3 origin;
    [Header("Random Size")]
    public Vector2 sizeRange = new Vector2(1.0f, 3.0f); // Min/Max scale range
    public void Initialize(Vector3 center)
    {
        origin = center;
        orbitRadius = initialOrbitRadius;
        float randomScale = Random.Range(sizeRange.x, sizeRange.y);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);
        // Ensure correct starting position
        Vector3 direction = (transform.position - origin).normalized;
        transform.position = origin + direction * orbitRadius;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        orbitRadius += orbitExpandSpeed * Time.deltaTime;

        Vector3 direction = (transform.position - origin).normalized;
        transform.position = origin + direction * orbitRadius;

        transform.RotateAround(origin, Vector3.forward, rotateSpeed * Time.deltaTime);

        transform.position += transform.up * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<HeroStats>().GodMode)
                return;

            other.GetComponent<HeroStats>().Health -= 1;
            Destroy(gameObject);
        }
        else if (other.name.Contains("Wall"))
        {
            Destroy(gameObject);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;

[System.Serializable]
public class GhostRingConfig
{
    public int ghostCount = 8;
}

public class GhostHordeManager : MonoBehaviour
{
    [Header("Ghost Horde Settings")]
    public GameObject ghostPrefab;
    public List<GhostRingConfig> rings = new List<GhostRingConfig>();
    public Vector3 centerOffset = new Vector3(0f, 0f, 5f); // Push ghosts to background visually
    public float baseOrbitRadius = 10f;
    public float ringSpacing = 4f;
    public float orbitSpeed = 10f;
    public bool rotateGhosts = true;

    // Internal data
    private List<GameObject> ghostInstances = new List<GameObject>();
    private List<float> ghostAngles = new List<float>();
    private List<float> orbitRadii = new List<float>();
    public Vector3 centerPoint;

    void Start()
    {
        if (gameObject.name == "BossReaper(Clone)")
        {
            centerPoint = gameObject.transform.position + centerOffset;
        }
        else
        {
            return;
        }

        for (int ringIndex = 0; ringIndex < rings.Count; ringIndex++)
        {
            GhostRingConfig ring = rings[ringIndex];
            float radius = baseOrbitRadius + ringIndex * ringSpacing;

            for (int i = 0; i < ring.ghostCount; i++)
            {
                float angle = i * 360f / ring.ghostCount;
                float rad = angle * Mathf.Deg2Rad;

                GameObject ghost = Instantiate(ghostPrefab, transform);
                ghostInstances.Add(ghost);
                ghostAngles.Add(angle);
                orbitRadii.Add(radius);

                Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
                ghost.transform.position = centerPoint + offset;
            }
        }
    }

    void Update()
    {
        if (!rotateGhosts) return;

        for (int i = 0; i < ghostInstances.Count; i++)
        {
            ghostAngles[i] += orbitSpeed * Time.deltaTime;
            float rad = ghostAngles[i] * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(rad) * orbitRadii[i],
                Mathf.Sin(rad) * orbitRadii[i],
                0
            );

            ghostInstances[i].transform.position = centerPoint + offset;
        }
    }
}

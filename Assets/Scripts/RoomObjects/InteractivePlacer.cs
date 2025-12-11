using System.Collections.Generic;
using UnityEngine;

public class InteractivePlacer : MonoBehaviour
{
    [Header("Interactive Prefabs")]
    public List<GameObject> interactivePrefabs;

    public GameObject[] monsterHouseBait;

    [Header("Trap Spawn Settings")]
    public int minTrapsPerRoom = 0;
    public int maxTrapsPerRoom = 2;

    [Tooltip("Min distance between traps")]
    public float trapSeparationRadius = 0.5f;

    public LayerMask trapLayerMask; // Make sure traps are on this layer

    private List<Vector3> debugTrapPositions = new List<Vector3>();

    [Header("Item Layer Check")]
    public LayerMask itemLayerMask; // Add this layer to detect item positions

    public void PlaceInteractives(Dictionary<Vector2Int, GameObject> placedRooms, List<Vector2Int> roomPlacementOrder)
    {
        debugTrapPositions.Clear(); // Clear old gizmo positions

        if (interactivePrefabs.Count == 0) return;

        List<Vector2Int> roomCandidates = new List<Vector2Int>(roomPlacementOrder);
        roomCandidates.Remove(Vector2Int.zero); // Skip origin
        Shuffle(roomCandidates);

        foreach (Vector2Int pos in roomCandidates)
        {
            if (!placedRooms.ContainsKey(pos)) continue;

            GameObject room = placedRooms[pos];

            // === MonsterHouse trap logic ===
            if (room.CompareTag("MonsterHouse"))
            {
                PlaceMonsterHouseTrap(room);
                Debug.Log("MONSTER HOUSE TRIGGER IN: " + room.name);
                continue;
            }
            if (room.CompareTag("BossRoom") || room.CompareTag("SafeRoom"))
            {
                Debug.Log("Skipping trap placement in Boss Room: " + room.name);
                continue;
            }

            // === Normal interactive trap placement ===
            int trapCount = Random.Range(minTrapsPerRoom, maxTrapsPerRoom + 1);
            int placed = 0;
            int attempts = 0;

            while (placed < trapCount && attempts < 15)
            {
                attempts++;

                GameObject prefab = interactivePrefabs[Random.Range(0, interactivePrefabs.Count)];
                Vector3 candidatePos = room.transform.position + new Vector3(
                    Random.Range(-1.5f, 1.5f),
                    Random.Range(-1.5f, 1.5f),
                    0
                );

                // Check for nearby traps
                // Check for nearby traps
                Collider2D trapHit = Physics2D.OverlapCircle(candidatePos, trapSeparationRadius, trapLayerMask);
                if (trapHit != null)
                {
                    Debug.Log($"[TrapOverlap] Blocked by trap: {trapHit.name} at {candidatePos}");
                    continue;
                }

                // Check for nearby items
                Collider2D itemHit = Physics2D.OverlapCircle(candidatePos, trapSeparationRadius, itemLayerMask);
                if (itemHit != null)
                {
                    Debug.Log($"[ItemOverlap] Blocked by item: {itemHit.name} at {candidatePos}");
                    continue;
                }


                Instantiate(prefab, candidatePos, Quaternion.identity);
                debugTrapPositions.Add(candidatePos);
                placed++;
            }
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.yellow;
        foreach (Vector3 pos in debugTrapPositions)
        {
            Gizmos.DrawWireSphere(pos, trapSeparationRadius);
        }
#endif
    }

    void PlaceMonsterHouseTrap(GameObject room)
    {
        Vector3 center = room.transform.position;

        int index = Random.Range(0, monsterHouseBait.Length);
        GameObject bait = Instantiate(monsterHouseBait[index], center, Quaternion.identity);
        bait.name = "MonsterHouseBait";

        MonsterHouseTrigger trigger = bait.GetComponent<MonsterHouseTrigger>();
        if (trigger == null)
            trigger = bait.AddComponent<MonsterHouseTrigger>();

        trigger.enemyBucket = FindObjectOfType<RoomGenerator>()?.enemyBucket;

        debugTrapPositions.Add(center);
    }



}

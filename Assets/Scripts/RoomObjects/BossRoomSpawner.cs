using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomSpawner : MonoBehaviour
{
    public GameObject bossPrefab;

    void Start()
    {
        if (bossPrefab != null)
        {
            Vector3 spawnPos = transform.position;

            GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            Transform waypoints = transform.Find("Waypoints");
            if (waypoints != null)
            {
                Debug.Log("Waypoints found");
                // Find Waypoints under this room
                if (boss.name.Contains("Reaper"))
                {
                    ReaperTeleport teleportEffect = boss.GetComponent<ReaperTeleport>();
                    if (teleportEffect != null)
                    {
                        teleportEffect.SetWaypointsParent(waypoints);
                    }
                }
                else if (boss.name.Contains("FireDancer"))
                {
                    FireDancer fireDancer = boss.GetComponent<FireDancer>();
                    fireDancer.waypointsParent = waypoints;
                }
            }
            else
            {
                Debug.LogWarning("[BossRoomSpawner] No 'Waypoints' object found in BossRoom.");
            }

        }
    }
}

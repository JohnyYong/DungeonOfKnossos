/*******************************************************************************
File:      PortalLogic.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component handles the collision logic for portals to reset the level
    when colliding with the player.

*******************************************************************************/
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalLogic : MonoBehaviour
{
    private PlayerRecordsTracker playerRecordsTracker;

    void Start()
    {
        playerRecordsTracker = GameObject.FindGameObjectWithTag("PlayerRecordTracker").GetComponent<PlayerRecordsTracker>();
        if (playerRecordsTracker == null)
        {
            Debug.Log("Can't find player tracker");
            return;
        }


    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var hero = collision.GetComponent<HeroStats>();
        if (hero != null)
        {
            playerRecordsTracker.SetPlayerLevelClear(playerRecordsTracker.GetPlayerLevelClear() + 1);
            Debug.Log("Player Level Clear Count: " + playerRecordsTracker.GetPlayerLevelClear());
            var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }
    }
}

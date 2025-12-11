/*******************************************************************************
File:      HeroStats.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component is keeps track of all relevant hero stats. It also handles
    collisions with objects that would modify any stat.

    - MaxHealth = 3
    - Power = 1

*******************************************************************************/
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeroStats : MonoBehaviour
{
    //Hero Stats
    public GameObject MainCameraPrefab;
    public GameObject UiCanvasPrefab;
    private UiStatsDisplay HeroStatsDisplay;
    private PlayerRecordsTracker tracker;
    public bool GodMode = false;
    public int StartingHealth = 3;
    public int originalBulletPower = 1;
    public int MaxHealth
    {
        get { return _MaxHealth; }

        set
        {
            HeroStatsDisplay.HealthBarDisplay.MaxHealth = value;
            _MaxHealth = value;
        }
    }
    private int _MaxHealth;

    public int Health
    {
        get { return _Health; }

        set
        {
            HeroStatsDisplay.HealthBarDisplay.Health = value;
            _Health = value;
        }

    }
    private int _Health;

    public int StartingSilverKeys = 0;
    public int SilverKeys
    {
        get { return _SilverKeys; }
        set
        {
            HeroStatsDisplay.SilverKeyDisplay.text = value.ToString();
            _SilverKeys = value;
        }
    }
    private int _SilverKeys;

    public int StartingGoldKeys = 0;
    public int GoldKeys
    {
        get { return _GoldKeys; }
        set
        {
            HeroStatsDisplay.GoldKeyDisplay.text = value.ToString();
            _GoldKeys = value;
        }
    }

    private int _GoldKeys;

    public int StartingPurpleKeys = 0;
    public int PurpleKeys
    {
        get { return _PurpleKeys; }
        set
        {
            HeroStatsDisplay.PurpleKeyDisplay.text = value.ToString();
            _PurpleKeys = value;
        }
    }

    private int _PurpleKeys;


    public int StartingPower = 1;
    public int Power
    {
        get { return _Power; }
        set
        {
            HeroStatsDisplay.PowerDisplay.text = value.ToString();
            _Power = value;
        }
    }
    private int _Power;

    // Start is called before the first frame update
    void Start()
    {
        //Spawn canvas
        var canvas = Instantiate(UiCanvasPrefab);
        HeroStatsDisplay = canvas.GetComponentInChildren<UiStatsDisplay>();
        tracker = GameObject.FindGameObjectWithTag("PlayerRecordTracker").GetComponent<PlayerRecordsTracker>();
        //Spawn main camera
        var cam = Camera.main;
        cam.GetComponent<ObjectFollow>().ObjectToFollow = transform;

        //Initialize stats
        MaxHealth = StartingHealth;
        Health = MaxHealth;
        SilverKeys = StartingSilverKeys;
        GoldKeys = StartingGoldKeys;
        PurpleKeys = StartingPurpleKeys;
        Power = StartingPower;

    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0)
        {
            gameObject.SetActive(false);
            tracker.SetPlayerDeathCount(tracker.GetPlayerDeathCount() + 1);
            Debug.Log("Player Death Count: " + tracker.GetPlayerDeathCount());
            Invoke("ResetLevel", 1.5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        MonsterHouseTrigger trap = collision.GetComponent<MonsterHouseTrigger>();
        if (trap != null)
        {
            trap.TriggerAmbush();
            var collect = collision.gameObject.GetComponent<CollectibleLogic>();
            if (collect != null)
            {

                switch (collect.Type)
                {
                    case CollectibleTypes.HealthBoost:
                        ++MaxHealth;
                        Health = MaxHealth;
                        break;
                    case CollectibleTypes.PowerBoost:
                        ++Power;
                        break;
                    default:
                        Debug.Log("UNKNOWN BAIT");
                        break;
                }   
            }

            Destroy(trap);
            return;
        }

        //Check collision against collectibles
        var collectible = collision.gameObject.GetComponent<CollectibleLogic>();
        if (collectible != null)
        {
            //Increment relevant stat baed on Collectible type
            switch (collectible.Type)
            {
                case CollectibleTypes.HealthBoost:
                    ++MaxHealth;
                    Health = MaxHealth;
                    break;
                case CollectibleTypes.SilverKey:
                    ++SilverKeys;
                    break;
                case CollectibleTypes.GoldKey:
                    ++GoldKeys;
                    break;
                case CollectibleTypes.PurpleKey:
                    ++PurpleKeys;
                    break;
                case CollectibleTypes.PowerBoost:
                    ++Power;
                    break;
                case CollectibleTypes.Heart:
                    if (Health == MaxHealth)
                        return;
                    ++Health;
                    break;
            }

            //Destroy collectible
            Destroy(collectible.gameObject);

        }//Collectibles End

        //Check collsion against enemy bullets
        var bullet = collision.GetComponent<BulletLogic>();
        if (bullet != null && bullet.Team == Teams.Enemy)
        {
            if (!GodMode)
                Health -= bullet.Power;
        }
    }

    void ResetLevel()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}

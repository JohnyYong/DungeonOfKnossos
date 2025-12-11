/*******************************************************************************
File:      HeroLogic.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component is keeps track of all relevant hero stats.

    - Starting Level = 1
    - MaxHealth = 2 + Level
    - Power = Level

*******************************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroLogic : MonoBehaviour
{
    //Hero Stats
    public GameObject HealthIconPrefab;
    public GameObject HealthDisplayPanel;
    public int MaxHealth
    {
        get { return _MaxHealth; }

        set
        {
            //Recreate all hearts icons
            for (int i = 0; i < HealthDisplayPanel.transform.childCount; ++i)
            {
                Destroy(HealthDisplayPanel.transform.GetChild(i).gameObject);
            }
            Hearts.Clear();

            for (int i = 0; i < value; ++i)
            {
                //Parent new icons to the panel
                var obj = Instantiate(HealthIconPrefab, HealthDisplayPanel.transform);
                Hearts.Add(obj.GetComponent<Image>());
            }

            _MaxHealth = value;
        }
    }
    private int _MaxHealth = 0;
    private List<Image> Hearts = new List<Image>();


    public int Health
    {
        get { return _Health; }

        set
        {
            //Ignore values out of bounds
            if (value > MaxHealth || value < 0)
                return;

            //Color health icons based on new health
            for (int i = Hearts.Count - 1; i >= 0; --i)
            {
                if (i < value)
                    Hearts[i].color = Color.red;
                else
                    Hearts[i].color = Color.grey;
            }

            _Health = value;
        }

    }
    private int _Health;

    public int SilverKeys
    {
        get { return _SilverKeys; }
        set
        {
            SilverKeysDisplay.text = value.ToString();
            _SilverKeys = value;
        }
    }
    private int _SilverKeys = 0;
    public TextMeshProUGUI SilverKeysDisplay;

    public int GoldKeys
    {
        get { return _GoldKeys; }
        set
        {
            GoldKeysDisplay.text = value.ToString();
            _GoldKeys = value;
        }
    }
    private int _GoldKeys = 0;
    public TextMeshProUGUI GoldKeysDisplay;

    public int Power
    {
        get { return _Power; }
        set
        {
            PowerDisplay.text = value.ToString();
            _Power = value;
        }
    }
    private int _Power = 1;
    public TextMeshProUGUI PowerDisplay;

    // Start is called before the first frame update
    void Start()
    {
        MaxHealth = 3;
        Health = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {

    }
}

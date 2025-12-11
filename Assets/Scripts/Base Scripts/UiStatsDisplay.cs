/*******************************************************************************
File:      UiStatsDisplay.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component holds references to all relevant Ui display objects so that
    they may be easily accessible by the HeroStats component.

*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UiStatsDisplay : MonoBehaviour
{
    public HealthBar HealthBarDisplay;
    public TextMeshProUGUI PowerDisplay;
    public TextMeshProUGUI SilverKeyDisplay;
    public TextMeshProUGUI GoldKeyDisplay;
    public TextMeshProUGUI PurpleKeyDisplay;
    public TextMeshProUGUI fairyText;
    public static UiStatsDisplay Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        HideFairyMessage();
    }
    public void ShowFairyMessage(string message)
    {
        fairyText.text = message;
        fairyText.gameObject.SetActive(true);
    }

    public void HideFairyMessage()
    {
        fairyText.gameObject.SetActive(false);
    }
}

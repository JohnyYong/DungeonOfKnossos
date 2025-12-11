/*******************************************************************************
File:      CameraZoomAndRestart.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component is added to the camera prefab and allows the player to control
    the zoom and restart the level (used for help with grading).

*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraZoomAndRestart : MonoBehaviour
{
    public float MinSize = 5f;
    public float MaxSize = 25f;

    private Camera Cam;

    // Start is called before the first frame update
    void Start()
    {
        Cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.mouseScrollDelta.y > 0)
        {
            Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize - 1f, MinSize, MaxSize);
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.mouseScrollDelta.y < 0)
        {
            Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize + 1f, MinSize, MaxSize);
        }


        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}

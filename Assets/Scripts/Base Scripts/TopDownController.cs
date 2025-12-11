/*******************************************************************************
File:      TopDownController.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2016
Course:    CS186
Section:   Z

Description:
    This component is responsible for all the movement actions for a top down
    character.

*******************************************************************************/
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TopDownController : MonoBehaviour
{
    //Character's Move Speed
    public float Speed = 5f;

    //Private References
    private Rigidbody2D RB;
    private bool isInverted = false;
    private float invertedDuration = 0f;
    private bool isInputLocked = false;

    private TemporalFairyCompanion fairyCompanion;
    private HeroStats heroStats;

    private float originalZoom = 6f;
    private float maxZoom = 100f;
    private bool zoomingBack = false;

    public void InvertControls(float duration)
    {
        if (!heroStats.GodMode)
        {
            isInverted = true;
            invertedDuration = duration;
        }
    }
    public void LockInput(float duration)
    {
        StartCoroutine(LockInputCoroutine(duration));
    }

    IEnumerator LockInputCoroutine(float time)
    {
        isInputLocked = true;
        yield return new WaitForSeconds(time);
        isInputLocked = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody2D>();
        heroStats = GetComponent<HeroStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isInputLocked)
        {
            RB.linearVelocity = Vector2.zero;
            return;
        }


        if (invertedDuration > 0f)
        {
            invertedDuration -= Time.deltaTime;
            if (invertedDuration <= 0f)
                isInverted = false;
        }
        //Reset direction every frame
        Vector2 dir = Vector2.zero;

        //Determine movement direction based on input
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            dir += isInverted ? Vector2.down : Vector2.up;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            dir += isInverted ? Vector2.right : Vector2.left;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            dir += isInverted ? Vector2.up : Vector2.down;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            dir += isInverted ? Vector2.left : Vector2.right;


        //Apply velocity
        if (fairyCompanion == null)
        {
            GameObject fairyObj = GameObject.FindGameObjectWithTag("Fairy");
            if (fairyObj != null)
                fairyCompanion = fairyObj.GetComponent<TemporalFairyCompanion>();
        }


        bool useUnscaled = fairyCompanion != null && fairyCompanion.IsSlowingTime();

        if (useUnscaled)
        {
            Vector3 movement = dir.normalized * Speed * Time.unscaledDeltaTime;
            transform.position += movement;
            //Debug.Log("Using Unscaled");
        }
        else
        {
            Vector3 movement = dir.normalized * Speed;
            // Regular velocity movement
            RB.linearVelocity = movement;
        }
        //For testing CameraShakeLogic
        if (Input.GetKey(KeyCode.U))
        {
            CameraShake.Instance.shakePreset = CameraShake.ShakePreset.ExtraSmall;
            CameraShake.Instance.TriggerShake();
        }
        if (Input.GetKey(KeyCode.I))
        {
            CameraShake.Instance.shakePreset = CameraShake.ShakePreset.Small;
            CameraShake.Instance.TriggerShake();
        }
        if (Input.GetKey(KeyCode.J))
        {
            CameraShake.Instance.shakePreset = CameraShake.ShakePreset.Medium;
            CameraShake.Instance.TriggerShake(); CameraShake.Instance.TriggerShake();
        }
        if (Input.GetKey(KeyCode.K))
        {
            CameraShake.Instance.shakePreset = CameraShake.ShakePreset.Large;
            CameraShake.Instance.TriggerShake();
        }

        if (Input.GetKey(KeyCode.F1))
        {
            var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            Camera camera = Camera.main;
            if (!zoomingBack)
            {
                camera.orthographicSize += 20f;
                if (camera.orthographicSize >= maxZoom)
                {
                    zoomingBack = true;
                }
            }
        }

        if (zoomingBack)
        {
            Camera camera = Camera.main;
            camera.orthographicSize = originalZoom;
            if (Mathf.Approximately(camera.orthographicSize, originalZoom))
            {
                zoomingBack = false;
            }
        }


        if (Input.GetKeyDown(KeyCode.F3))
        {
            heroStats.GodMode = !heroStats.GodMode;
            Debug.Log($"GodMode: {heroStats.GodMode}");
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            Camera camera = Camera.main;
            camera.orthographicSize = originalZoom;
        }

    }
}

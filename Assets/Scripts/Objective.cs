using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Objective : MonoBehaviour
{
    protected GameManager gameManager;
    protected MissionManager missionManager;
    protected ScreenManager screenManager;

    PlayerController playerController;
    GameObject playerObject;

    //[HideInInspector]
    public GameObject subjectObject;

    public string[] preStrings;
    public string[] postStrings;

    protected bool managed = false;
    protected bool hasPreStrings = false;
    protected bool hasPostStrings = false;

    protected Shader tpShader;
    protected Shader normalShader;

    protected Vector3 spawnPoint;

    public bool activateDoor = false;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        playerObject = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RunComplete()
    {
        if (managed == true)
        {

            Debug.Log(subjectObject.name + "/" + postStrings.Length);
            missionManager.NextObjective(subjectObject, postStrings);
        }

        else if (hasPostStrings)
        {
            string[] output = new string[postStrings.Length + 1];
            output[0] = "dialogue";

            Array.Copy(postStrings, 0, output, 1, postStrings.Length);
            screenManager.SetScreen(output);
        }
    }

    public string[] GetPreStrings()
    {
        return preStrings;
    }

    public string[] GetPostStrings()
    {
        return postStrings;
    }

    public void ActivateMe()
    {
        activateDoor = true;
    }
}

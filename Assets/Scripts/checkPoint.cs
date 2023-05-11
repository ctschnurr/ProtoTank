using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class checkPoint : Objective
{
    GameObject parent;
    Vector3 respawnPoint;
    Objective checkpoint;

    Vector3 sizeUp;
    Vector3 sizeDown;

    Color lerpedColor = Color.white;

    Vector3 scaleChange;
    float speed = 0.5f;
    bool expand = true;
    public bool fadeOut = false;

    Color tempcolor;
    Color savecolor;

    AudioSource checkpointSound;

    public enum State
    {
        Indy,
        Managed
    }

    public State state = State.Indy;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        missionManager = GameObject.Find("MissionManager").GetComponent<MissionManager>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();

        subjectObject = transform.gameObject;
        respawnPoint = transform.position;

        speed *= Time.deltaTime;
        scaleChange = new Vector3(speed, speed, speed);
        tempcolor = GetComponent<Renderer>().material.color;
        savecolor = tempcolor;

        checkpointSound = GetComponent<AudioSource>();

        if (transform.parent != null)
        {
            if (transform.parent.gameObject.tag == "MissionGroup" || transform.parent.gameObject.tag == "ObjectiveGroup") managed = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localScale.x > 2f) expand = false; 
        if (transform.localScale.x < 1.75f) expand = true;

        if (expand) transform.localScale += scaleChange;
        if (!expand) transform.localScale -= scaleChange;

        if (fadeOut)
        {
            tempcolor = GetComponent<Renderer>().material.color;
            tempcolor.a = Mathf.MoveTowards(tempcolor.a, 0f, 0.005f);
            GetComponent<Renderer>().material.color = tempcolor;
        }

        if (GetComponent<Renderer>().material.color.a == 0f)
        {
            fadeOut = false;
            GetComponent<Renderer>().material.color = savecolor;

            RunComplete();

            subjectObject.SetActive(false);
        }

    }

    void TriggerFadeOut()
    {
        fadeOut = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            checkpointSound.Play();

            PlayerController.SetRespawn(respawnPoint);

            //player.SetMessage("Checkpoint!");
            //player.ShowText();

            Invoke("TriggerFadeOut", 0.4f);
        }
    }
}

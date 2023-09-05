using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DoorManager : Objective
{

    public enum State
    {
        open,
        closed
    }

    State state = State.closed;
    State storage = State.closed;

    public float speed = 2f;

    GameObject doorL;
    GameObject doorR;
    GameObject parent;

    private Vector3 goLeft = new Vector3(0, 0, 0);
    private Vector3 goRight = new Vector3(0, 0, 0);
    private Vector3 startLeft = new Vector3(0, 0, 0);
    private Vector3 startRight = new Vector3(0, 0, 0);

    public bool done = false;
    public bool complete = false;
    bool playOnce = false;

    AudioSource doorSound;

    // Start is called before the first frame update
    void Start()
    {
        missionManager = GameObject.Find("MissionManager").GetComponent<MissionManager>();

        doorSound = GetComponent<AudioSource>();

        parent = transform.gameObject;
        subjectObject = transform.gameObject;

        doorL = parent.transform.Find("DoorL").gameObject;
        doorR = parent.transform.Find("DoorR").gameObject;

        startLeft = doorL.transform.localPosition;
        startRight = doorR.transform.localPosition;

        goLeft = startLeft;
        goRight = startRight;

        goLeft.z = startLeft.z - 3f;
        goRight.z = startRight.z + 3f;

        if (transform.parent != null)
        {
            if (transform.parent.gameObject.tag == "MissionGroup" || transform.parent.gameObject.tag == "ObjectiveGroup") managed = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!complete)
        {
            if (activateDoor && !done)
            {
                if (!playOnce)
                {
                    doorSound.Play();
                    playOnce = true;
                }

                if (doorL.transform.localPosition.z != goLeft.z)
                {
                    doorL.transform.localPosition = Vector3.MoveTowards(doorL.transform.localPosition, goLeft, Time.deltaTime * speed);
                    doorR.transform.localPosition = Vector3.MoveTowards(doorR.transform.localPosition, goRight, Time.deltaTime * speed);
                }
                else done = true;
            }

            if (activateDoor && done)
            {
                RunComplete();
                complete = true;
            }
        }
    }

    void SetState()
    {
        state = storage;
    }

    void Reset()
    {
        doorL.transform.localPosition = startLeft;
        doorR.transform.localPosition = startRight;

        done = false;
        complete = false;
        activateDoor = false;

    }
}

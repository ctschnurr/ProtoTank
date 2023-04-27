using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MissionManager : MonoBehaviour
{
    GameManager gameManager;
    DialogueManager dialogueManager;
    ScreenManager screenManager;

    GameObject parent;
    GameObject nextCheckPoint;
    GameObject reference;

    GameObject missionFolder1;
    int numberOfMissions;

    public enum Mission
    {
        mission1,
        mission2,
        mission3
    }

    // Mission mission = Mission.mission1;

    int currentMission = 0;
    float timer = 2;

    bool missionStart = false;
    bool missionComplete = false;

    // string[] dialogue;
    List<GameObject>[] missions;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();
        // dialogue = new string[2];
        // 
        // dialogue[0] = "This is a test message!";
        // dialogue[1] = "Please disregard!";

        parent = transform.gameObject;

        // setup of Mission 1 objects
        //mission1Objects = new List<GameObject>();

        numberOfMissions = parent.transform.childCount;
        missions = new List<GameObject>[numberOfMissions];

        for (int i = 0; i < numberOfMissions; i++)
        {
            missions[i] = new List<GameObject>();
            Transform missionFolderTransform = parent.transform.GetChild(i);
            GameObject missionFolder = missionFolderTransform.gameObject;

            int numberOfObjectives = missionFolder.transform.childCount;

            for (int j = 0; j < numberOfObjectives; j++)
            {
                Transform missionObjectiveTransform = missionFolder.transform.GetChild(j);
                GameObject missionObjective = missionObjectiveTransform.gameObject;

                missions[i].Add(missionObjective);
            }
        }

        List<GameObject> holder = missions[currentMission];

        for (int i = 1; i < missions[currentMission].Count; i++)
        {
            GameObject deactivateMe = holder[i];
            deactivateMe.SetActive(false);
        }
        //-----


    }

    // Update is called once per frame
    void Update()
    {
        if (missionStart)
        {
            if (timer > 0) timer -= Time.deltaTime;
            if (timer < 0)
            {
                string message = "For your first mission, we will simply run through some exercises!";
                screenManager.SetScreen(ScreenManager.Screen.missionStart);
                dialogueManager.InfoBox(message);
                missionStart = false;
                timer = 6;
            }
        }
        else if(missionComplete)
        {
            string[] message = new string[2];
            message[0] = "Fantastic work! You'll be quite an asset to the company!";
            message[1] = "Next we'll run through some more advanced manuvers!";
            dialogueManager.StartDialogue(message);
            if (timer > 0) timer -= Time.deltaTime;
            if (timer < 0)
            {
                string message = "Click CONTINUE to move on to the next mission.";
                screenManager.SetScreen(ScreenManager.Screen.missionComplete);

                Time.timeScale = 0;
                player.SetState(PlayerController.State.controlDisabled);

                dialogueManager.InfoBox(message);
                missionComplete = false;
                currentMission++;
            }
        }
    }

    void CountDown(float time)
    {
        timer = time;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }

    public GameObject GetNextCheckpoint()
    {
        List<GameObject> holder = missions[currentMission];
        return holder[0];
    }

    public void NextObjective(GameObject input, string[] dialogue)
    {
        reference = input;
        //reference.SetActive(false);
        missions[currentMission].Remove(reference);

        dialogueManager.StartDialogue(dialogue);

        if (missions[currentMission].Count != 0)
        {
            List<GameObject> refList = missions[currentMission];
            refList[0].SetActive(true);
        }
        else missionComplete = true;
    }

    public void StartMission()
    {
        missionStart = true;
    }
}

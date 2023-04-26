using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MissionManager : MonoBehaviour
{
    DialogueManager dialogueManager;

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
    float timer = 120.0f;

    // string[] dialogue;
    List<GameObject>[] missions;

    // Start is called before the first frame update
    void Start()
    {
        dialogueManager = GameObject.Find("DialogueScreen").GetComponent<DialogueManager>();
        // dialogue = new string[2];
        // 
        // dialogue[0] = "This is a test message!";
        // dialogue[1] = "Please disregard!";

        timer *= Time.deltaTime;

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
                Debug.Log("Booner");
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
        List<GameObject> missionReference = missions[currentMission];

        switch (currentMission)
        {
            case 0:
                // int numberOfStages = missionReference.Count;



                // if (missionStage == 0)
                // {
                //     timer -= Time.deltaTime;
                // 
                //     if (timer <= 0)
                //     {
                //         Array.Resize(ref dialogue, 3);
                //         dialogue[0] = "Welcome to Mission 1, recruit!";
                //         dialogue[1] = "Lets get ready to run through your paces.";
                //         dialogue[2] = "Time to begin!";
                // 
                //         dialogueManager.StartDialogue(dialogue);
                // 
                //         missionStage++;
                //         timer = 120.0f;
                //     }
                // }
                // else if (missionStage == 1)
                // {
                //     if (checkPoint1) timer -= Time.deltaTime;
                // 
                // 
                // 
                // }

                break;
        }
    }

    void CountDown(float time)
    {
        timer = time;
        while (timer > 0)
        {
            Debug.Log(timer);
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
        reference.SetActive(false);
        missions[currentMission].Remove(reference);

        if (missions[currentMission].Count != 0)
        {
            List<GameObject> refList = missions[currentMission];
            refList[0].SetActive(true);

            dialogueManager.StartDialogue(dialogue);
        }
    }
}

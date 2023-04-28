using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MissionManager : MonoBehaviour
{
    DialogueManager dialogueManager;


    GameManager gameManager;
    ScreenManager screenManager;
    PlayerController player;

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
    public int stage = 0;
    float timer = 2;

    bool missionStart = false;
    static bool missionComplete = false;

    // string[] dialogue;
    List<GameObject>[] missions;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();

        parent = transform.gameObject;

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
            switch (stage)
            {
                case 0:
                    if (timer > 0) timer -= Time.deltaTime;
                    if (timer < 0)
                    {
                        NextStage();
                        string message = "For your first mission, we will simply run through some exercises!";
                        screenManager.SetScreen(ScreenManager.Screen.missionStart);
                        dialogueManager.InfoBox(message);
                        timer = 4;
                    }
                    break;

                case 1:
                    // basically just waiting for next stage
                    break;

                case 2:
                    if (timer > 0) timer -= Time.deltaTime;
                    if (timer < 0)
                    {
                        screenManager.SetScreen(ScreenManager.Screen.HUD);
                        missionStart = false;
                        timer = 10;
                    }
                    break;

            }

        }
        else if(missionComplete)
        {
            if (timer > 0) timer -= Time.deltaTime;
            if (timer < 0)
            {
                string message2 = "Click CONTINUE to move on to the next mission.";
                screenManager.SetScreen(ScreenManager.Screen.missionComplete);

                Time.timeScale = 0;
                player.SetState(PlayerController.State.controlDisabled);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                dialogueManager.InfoBox(message2);
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
        else
        {
            missionComplete = true;
        }
    }

    public void StartMission()
    {
        missionStart = true;
    }

    public static void EndMission(object sender, EventArgs e)
    {
        missionComplete = true;
    }

    public void NextStage()
    {
        stage++;
    }
}

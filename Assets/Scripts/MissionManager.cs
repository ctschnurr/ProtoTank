using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MissionManager : MonoBehaviour
{
    static DialogueManager dialogueManager;
    static GameManager gameManager;
    static ScreenManager screenManager;
    static PlayerController player;

    int endMissionDelay = 3;

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

    static int currentMission = 0;

    static int stage = 0;
    static float timer = 2;

    static string[] output;
    static bool countDown = false;

    static List<GameObject>[] missions;

    static bool missionStart = false;
    static bool missionEnd = false;
    static bool missionNext = false;

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

                missionObjective.SetActive(false);
            }
        }

        //-----

        DialogueManager.OnDialogueEnd += NextStage;
        // ScreenManager.OnFadeOutComplete += AdvanceMission;

    }

    // Update is called once per frame
    void Update()
    {
        if (countDown)
        {
            if (timer > 0) timer -= Time.deltaTime;
            if (timer < 0)
            {
                AdvanceMission();
                countDown = false;
            }
        }
    }

    public static void StartMission()
    {
        List<GameObject> holder = missions[currentMission];
        GameObject activateMe = holder[0];
        activateMe.SetActive(true);

        stage = 0;
        timer = 2;
        missionStart = true;
        countDown = true;
    }

    public static void AdvanceMission()
    {

        if (missionStart)
        {
            switch (stage)
            {
                case 0:
                    output = new string[2];
                    output[0] = "missionStart";
                    output[1] = "Mission " + (currentMission +1) + ": Lets run through some exercises!";
                    screenManager.SetScreen(output);

                    stage++;
                    break;

                case 1:
                    //output = new string[1];
                    //output[0] = "clear";
                    //screenManager.SetScreen(output);

                    gameManager.SetState(GameManager.State.active);

                    //timer = 1;
                    stage++;
                    break;

                case 2:
                    output = new string[2];
                    output[0] = "dialogue";
                    output[1] = "Please proceed to the first checkpoint. The blue nav arrow will point the way for you!";
                    screenManager.SetScreen(output);

                    stage++;
                    break;

                case 3:
                    output = new string[1];
                    output[0] = "HUD";
                    screenManager.SetScreen(output);
                    player.TogglePointer();

                    missionStart = false;
                    break;
            }
        }

        if (missionEnd)
        {
            switch (stage)
            {
                case 0:
                    output = new string[1];
                    output[0] = "HUD";
                    screenManager.SetScreen(output);

                    output = new string[2];
                    output[0] = "missionComplete";
                    output[1] = "Click CONTINUE to move on to the next mission.";
                    screenManager.SetScreen(output);

                    Time.timeScale = 0;
                    player.SetState(PlayerController.State.controlDisabled);

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    missionEnd = false;
                    break;
            }

        }

        if (missionNext)
        {

            switch (stage)
            {
                case 0:
                    Debug.Log("Reset");
                    player.Reset();
                    currentMission++;
                    stage++;
                    AdvanceMission();
                    break;

                case 1:
                    Debug.Log("Fade");
                    string[] output = new string[1];
                    output[0] = "black";
                    screenManager.SetScreen(output);

                    missionNext = false;
                    StartMission();
                    break;

            }

        }

    }

    public void EndMission()
    {
        timer = endMissionDelay;
        missionEnd = true;
        stage = 0;
    }

    public void NextMission()
    {
        string[] output = new string[1];

        output[0] = "black";
        screenManager.SetScreen(output);

        output[0] = "clear";
        screenManager.SetScreen(output);

        missionNext = true;
        stage = 0;
        timer = 2;
        countDown = true;

        Time.timeScale = 1;
    }

    public GameObject GetNextCheckpoint()
    {
        List<GameObject> holder = missions[currentMission];
        return holder[0];
    }

    public void NextObjective(GameObject input, string[] dialogue)
    {
        reference = input;
        missions[currentMission].Remove(reference);

        output = new string[dialogue.Length + 1];
        output[0] = "dialogue";

        Array.Copy(dialogue, 0, output, 1, dialogue.Length);

        screenManager.SetScreen(output);

        if (missions[currentMission].Count != 0)
        {
            List<GameObject> refList = missions[currentMission];
            refList[0].SetActive(true);
        }
        else
        {
            EndMission();
        }
    }

    static void NextStage()
    {
        if (missionStart) countDown = true;
    }
}

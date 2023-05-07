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
    static int currentMission = 0;

    // public enum Mission
    // {
    //     mission1,
    //     mission2,
    //     mission3
    // }

    public enum State
    {
        idle,
        missionStart,
        missionStop,
        missionNext,
        advanceMission
    }

    static State state = State.idle;

    static int stage = 0;
    static float timer = 2;

    static string[] output;
    static bool countDown = false;

    static List<GameObject>[] missions;

    static bool missionStart = false;
    static bool missionEnd = false;
    static bool missionNext = false;

    static Queue<State> missionQueue;

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
        ScreenManager.OnFadeOutComplete += NextStage;

        missionQueue = new Queue<State>();

    }

    // Update is called once per frame
    void Update()
    {
        if (missionQueue.Count != 0 && state == State.idle)
        {
            if (countDown)
            {
                if (timer > 0) timer -= Time.deltaTime;
                if (timer < 0)
                {
                    State nextState = missionQueue.Dequeue();
                    SetState(nextState);
                }
            }
        }
    }

    public static void StartMission()
    {
        List<GameObject> holder = missions[currentMission];
        GameObject activateMe = holder[0];
        activateMe.SetActive(true);

        stage = 0;
        timer = 1;
        missionStart = true;
        countDown = true;
        State next = State.missionStart;
        missionQueue.Enqueue(next);
    }

    void SetState(State input)
    {
        switch (input)
        {
            case State.missionStop:
                missionEnd = true;
                stage = 0;

                player.TogglePointer();
                break;

            case State.missionNext:
                missionNext = true;
                stage = 0;
                countDown = true;

                Time.timeScale = 1;
                break;
        }
        state = input;
        AdvanceMission();
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
                    output[1] = "Mission " + (currentMission +1) + ": \n\nWelcome to basic training! To start we're going to run through some exercises.";
                    screenManager.SetScreen(output);

                    stage++;
                    State next = State.advanceMission;
                    missionQueue.Enqueue(next);
                    break;

                case 1:
                    //output = new string[1];
                    //output[0] = "clear";
                    //screenManager.SetScreen(output);

                    gameManager.SetState(GameManager.State.active);

                    //timer = 1;
                    stage++;
                    next = State.advanceMission;
                    missionQueue.Enqueue(next);
                    break;

                case 2:
                    output = new string[2];
                    output[0] = "dialogue";
                    output[1] = "Let's fire up your HUD and activate your waypoint compass. Please proceed to the first checkpoint!";
                    screenManager.SetScreen(output);

                    stage++;
                    next = State.advanceMission;
                    missionQueue.Enqueue(next);
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
            output = new string[1];
            output[0] = "HUD";
            screenManager.SetScreen(output);

            output = new string[2];
            output[0] = "missionComplete";
            output[1] = "Good work! \n\n Time:\n Attempts:\n\nClick CONTINUE to move on to the next mission!";
            screenManager.SetScreen(output);

            gameManager.SetState(GameManager.State.inactive);
            player.SetState(PlayerController.State.controlDisabled);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            missionEnd = false;

        }

        if (missionNext)
        {

            switch (stage)
            {
                case 0:
                    string[] output = new string[1];

                    output[0] = "black";
                    screenManager.SetScreen(output);

                    output[0] = "clear";
                    screenManager.SetScreen(output);

                    timer = 2;
                    stage++;
                    state = State.idle;

                    State next = State.advanceMission;
                    missionQueue.Enqueue(next);
                    break;

                case 1:
                    player.Reset();
                    currentMission++;
                    stage++;
                    next = State.advanceMission;
                    missionQueue.Enqueue(next);
                    break;

                case 2:
                    output = new string[1];
                    output[0] = "black";
                    screenManager.SetScreen(output);

                    missionNext = false;
                    state = State.idle;
                    StartMission();
                    break;

            }

        }

    }

    public void EndMission()
    {
        timer = endMissionDelay;
        State next = State.missionStop;
        missionQueue.Enqueue(next);
    }

    public void NextMission()
    {
        State next = State.missionNext;
        missionQueue.Enqueue(next);
        state = State.idle;
    }

    public GameObject GetNextCheckpoint()
    {
        List<GameObject> holder = missions[currentMission];
        if (holder.Count == 0) return null;
        else return holder[0];
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
        state = State.idle;
        countDown = true;
    }
}

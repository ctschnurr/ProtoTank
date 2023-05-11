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

    static int endMissionDelay = 5;

    GameObject playerObj;
    static GameObject parent;
    GameObject nextCheckPoint;
    GameObject reference;

    GameObject missionFolder1;
    static int numberOfMissions;
    static int currentMission = 0;

    public enum State
    {
        idle,
        missionStart,
        missionComplete,
        missionFailed,
        missionNext,
        advanceMission
    }

    static string[] missionPreString;
    static string[] missionPostString;

    static State state = State.idle;

    static int stage = 0;
    static float timer = 2;

    static string[] output;
    static bool countDown = false;

    static List<GameObject>[] missions;

    static bool missionStart = false;
    static bool missionComplete = false;
    static bool missionFailed = false;
    static bool missionNext = false;

    static Queue<State> missionQueue;

    public static Objective objReference;

    public delegate void RunResetAction();
    public static event RunResetAction OnRunReset;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        playerObj = GameObject.Find("Player");

        parent = transform.gameObject;

        SetMissions();

        DialogueManager.OnDialogueEnd += NextStage;
        ScreenManager.OnFadeOutComplete += NextStage;
        PlayerController.OnPlayerDead += PlayerDead;

        missionQueue = new Queue<State>();

    }

    public static void SetMissions()
    {
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
        Objective reference = parent.transform.GetChild(currentMission).GetComponent<Objective>();
        missionPreString = reference.GetPreStrings();
        missionPostString = reference.GetPostStrings();

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
            case State.missionComplete:
                missionComplete = true;
                stage = 0;
                break;

            case State.missionFailed:
                missionFailed = true;
                stage = 0;
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
                    output[1] = "Mission " + (currentMission + 1) + ": \n\n" + missionPreString[0];
                    screenManager.SetScreen(output);

                    stage++;
                    State next = State.advanceMission;
                    missionQueue.Enqueue(next);
                    break;

                case 1:
                    gameManager.SetState(GameManager.State.active);

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

                    stage++;
                    next = State.advanceMission;
                    missionQueue.Enqueue(next);

                    state = State.idle;
                    break;

                case 4:
                    List<GameObject> holder = missions[currentMission];
                    GameObject activateMe = holder[0];
                    activateMe.SetActive(true);

                    objReference = activateMe.GetComponent<Objective>();
                    string[] dialogue = objReference.GetPreStrings();

                    output = new string[dialogue.Length + 1];
                    output[0] = "dialogue";

                    Array.Copy(dialogue, 0, output, 1, dialogue.Length);
                    screenManager.SetScreen(output);

                    missionStart = false;
                    state = State.idle;
                    break;
            }
        }

        if (missionComplete)
        {
            output = new string[1];
            output[0] = "HUD";
            screenManager.SetScreen(output);

            output = new string[2];
            output[0] = "missionComplete";
            output[1] = "\nGood work! \n\n Time:\n Attempts: \n\n" + missionPostString[0];
            screenManager.SetScreen(output);

            gameManager.SetState(GameManager.State.inactive);
            player.SetState(PlayerController.State.controlDisabled);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            currentMission++;

            missionComplete = false;
        }

        if (missionFailed)
        {
            output = new string[1];
            output[0] = "HUD";
            screenManager.SetScreen(output);

            output = new string[2];
            output[0] = "missionFailed";
            output[1] = "\nOh no!\n\nYour tank has been destroyed!\n\nChin up, soldier, we'll get 'em next time!";
            screenManager.SetScreen(output);

            gameManager.SetState(GameManager.State.inactive);
            player.SetState(PlayerController.State.controlDisabled);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            missionFailed = false;
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

                    // timer = 2;
                    // state = State.idle;
                    
                    stage++;

                    State next = State.advanceMission;
                    missionQueue.Enqueue(next);
                    break;

                case 1:
                    RunReset();
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

    public void NextMission()
    {
        State next = State.missionNext;
        missionQueue.Enqueue(next);
        state = State.idle;
    }

    static public void RunReset()
    {
        if (OnRunReset != null)
        {
            OnRunReset();
        }
        SetMissions();
    }

    public GameObject GetNextCheckpoint()
    {
        List<GameObject> holder = missions[currentMission];
        if (holder.Count == 0) return null;
        GameObject subject = holder[0];

        if (subject.tag == "ObjectiveGroup")
        {
            Transform objGroupTransform = subject.transform;
            Component[] objectives = subject.GetComponentsInChildren<Transform>();

            Transform nearest = null;
            float minDist = Mathf.Infinity;

            foreach (Transform objective in objectives)
            {
                float dist = Vector3.Distance(objective.position, player.transform.position);
                if (dist < minDist && objective.gameObject != subject)
                {
                    nearest = objective;
                    minDist = dist;
                }
            }

            subject = nearest.gameObject;
        }

        return subject;
    }

    public void NextObjective(GameObject input, string[] dialogue)
    {
        GameObject parent = input.transform.parent.gameObject;
        int left = 0;

        if (parent.tag == "ObjectiveGroup")
        {
            input.transform.SetParent(null);
            left = parent.transform.childCount;

            if (left > 0)
            {
                output = new string[dialogue.Length + 2];
                output[0] = "dialogue";
                output[dialogue.Length + 1] = left + " to go!";
                Array.Copy(dialogue, 0, output, 1, dialogue.Length);
                screenManager.SetScreen(output);
            }
            else
            {
                missions[currentMission].Remove(parent);
            }
        }
        else
        {
            reference = input;
            missions[currentMission].Remove(reference);

        }

        if (missions[currentMission].Count != 0)
        {
            if(parent.tag == "ObjectiveGroup" && left > 0)
            {



            }
            else
            {
                List<GameObject> refList = missions[currentMission];
                GameObject nextObj = refList[0];
                nextObj.SetActive(true);

                if (nextObj.tag == "ObjectiveGroup")
                {
                    Transform firstInGroup = nextObj.transform.GetChild(0);
                    objReference = firstInGroup.GetComponent<Objective>();
                    string[] preStrings = objReference.GetPreStrings();

                    int count = nextObj.transform.childCount;

                    if (preStrings != null)
                    {
                        output = new string[(dialogue.Length + preStrings.Length + 2)];
                        output[0] = "dialogue";

                        Array.Copy(dialogue, 0, output, 1, dialogue.Length);
                        output[dialogue.Length + 1] = "This next objective has " + count + " parts!";
                        Array.Copy(preStrings, 0, output, dialogue.Length + 2, preStrings.Length);

                        screenManager.SetScreen(output);
                    }
                    else
                    {
                        output = new string[(dialogue.Length + 2)];
                        output[0] = "dialogue";

                        Array.Copy(dialogue, 0, output, 1, dialogue.Length);

                        output[dialogue.Length + 1] = "This next objective has " + count + " parts!";
                        screenManager.SetScreen(output);
                    }
                }
                else
                {
                    objReference = nextObj.GetComponent<Objective>();
                    string[] preStrings = objReference.GetPreStrings();
                    if (preStrings != null)
                    {
                        output = new string[(dialogue.Length + preStrings.Length + 1)];
                        output[0] = "dialogue";

                        Array.Copy(dialogue, 0, output, 1, dialogue.Length);
                        Array.Copy(preStrings, 0, output, dialogue.Length + 1, preStrings.Length);
                        screenManager.SetScreen(output);
                    }
                    else
                    {
                        output = new string[(dialogue.Length + 1)];
                        output[0] = "dialogue";

                        Array.Copy(dialogue, 0, output, 1, dialogue.Length);
                        screenManager.SetScreen(output);
                    }

                }
            }
        }
        else
        {
            output = new string[(dialogue.Length + 1)];
            output[0] = "dialogue";

            Array.Copy(dialogue, 0, output, 1, dialogue.Length);
            screenManager.SetScreen(output);

            EndMission();
        }
    }

    public void EndMission()
    {
        timer = endMissionDelay;
        State next = State.missionComplete;
        missionQueue.Enqueue(next);
    }

    static void NextStage()
    {
        state = State.idle;
        countDown = true;
    }

    static void PlayerDead()
    {
        timer = 2;
        //countDown = true;
        State next = State.missionFailed;
        missionQueue.Enqueue(next);
    }
}

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

    int endMissionDelay = 5;

    GameObject playerObj;
    GameObject parent;
    GameObject nextCheckPoint;
    GameObject reference;

    GameObject missionFolder1;
    int numberOfMissions;
    static int currentMission = 0;

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

    public static Objective objReference;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        playerObj = GameObject.Find("Player");

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
            output[1] = "\nGood work! \n\n Time:\n Attempts:\n\nClick CONTINUE to move on to the next mission!";
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
        int left = parent.transform.childCount;
        left--;

        if (parent.tag == "ObjectiveGroup")
        {
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
            
            // output = new string[dialogue.Length + 1];
            // output[0] = "dialogue";
            // 
            // Array.Copy(dialogue, 0, output, 1, dialogue.Length);
            // screenManager.SetScreen(output);
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
        State next = State.missionStop;
        missionQueue.Enqueue(next);
    }

    static void NextStage()
    {
        state = State.idle;
        countDown = true;
    }
}

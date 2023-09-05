using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MissionManager : MonoBehaviour
{
    static DialogueManager dialogueManager;
    static GameManager gameManager;
    static ScreenManagerV2 screenManager;
    static PlayerController player;

    static int endMissionDelay = 3;

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
        advanceMission,
        gameEnd,
        quitToMenu
    }

    static string[] missionPreString;
    static string[] missionPostString;

    static State state = State.idle;
    public State test;

    static int stage = 0;
    static float timer = 2;

    static string[] output;
    static bool countDown = false;

    static List<GameObject>[] missions;

    static bool missionStart = false;
    static bool missionComplete = false;
    static bool missionFailed = false;
    static bool missionNext = false;
    static bool missionFinal = false;
    static bool gameEnd = false;
    static bool quit = false;

    static bool fadeIn = false;
    static AudioSource input;

    static Queue<State> missionQueue;

    public static Objective objReference;

    public delegate void RunResetAction();
    public static event RunResetAction OnRunReset;

    static AudioSource gameAmbience;

    static AudioClip ambience;

    GameManager.State gameState;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManagerV2>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        playerObj = GameObject.Find("Player");

        parent = transform.gameObject;

        SetMissions();

        DialogueManager.OnDialogueEnd += NextStage;
        ScreenManagerV2.OnFadeOutComplete += NextStage;
        PlayerController.OnPlayerDead += PlayerDead;

        missionQueue = new Queue<State>();

        gameAmbience = GameObject.Find("Player").GetComponent<AudioSource>();
        ambience = Resources.Load<AudioClip>("ambience2");
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

                if (missionObjective.name != "Gate") missionObjective.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // test = state;
        gameState = gameManager.GetState();

        if (gameState != GameManager.State.paused)
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

            if (fadeIn == true)
            {
                if (input.volume < 1)
                {
                    input.volume += 0.1f * (Time.deltaTime);
                }
                else fadeIn = false;
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

            case State.gameEnd:
                gameEnd = true;
                stage = 0;
                break;

            case State.quitToMenu:
                quit = true;
                stage = 0;
                break;
        }
        state = input;
        if (state != State.idle) AdvanceMission();
    }

    public static void AdvanceMission()
    {

        if (missionStart)
        {
            switch (stage)
            {
                case 0:
                    if (!gameAmbience.isPlaying)
                    {
                        gameAmbience.clip = ambience;
                        gameAmbience.loop = true;
                        gameAmbience.volume = 0;
                        gameAmbience.Play();
                        input = gameAmbience;
                        fadeIn = true;
                    }

                    // output = new string[2];
                    // if (currentMission + 1 == numberOfMissions) output[0] = "missionFinal";
                    // else output[0] = "missionStart";
                    // output[1] = "Mission " + (currentMission + 1) + ": \n\n" + missionPreString[0];
                    // screenManager.SetScreen(output);

                    output = new string[1];
                    output[0] = "Mission " + (currentMission + 1) + ": \n\n" + missionPreString[0];

                    if (currentMission + 1 == numberOfMissions) screenManager.SetScreen(ScreenManagerV2.Screen.missionFinal, output, false, 0);
                    else screenManager.SetScreen(ScreenManagerV2.Screen.missionStart, output, false, 0);

                    stage++;
                    break;

                case 1:
                    gameManager.SetState(GameManager.State.active);

                    stage++;
                    State next = State.advanceMission;
                    missionQueue.Enqueue(next);
                    break;

                case 2:
                    // output = new string[2];
                    // output[0] = "dialogue";
                    // output[1] = "Please standby while I activate your HUD and waypoint compass.";
                    // screenManager.SetScreen(output);

                    output = new string[1];
                    output[0] = "Please standby while I activate your HUD and waypoint compass.";
                    screenManager.SetScreen(ScreenManagerV2.Screen.dialogue, output, false, 0);

                    stage++;
                    next = State.advanceMission;
                    missionQueue.Enqueue(next);
                    break;

                case 3:
                    // output = new string[1];
                    // output[0] = "HUD";
                    // screenManager.SetScreen(output);

                    screenManager.SetScreen(ScreenManagerV2.Screen.HUD, null, false, 0);

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

                    string[] dialogue = new string[0];

                    objReference = activateMe.GetComponent<Objective>();
                    dialogue = objReference.GetPreStrings();

                    if (dialogue.Length != 0)
                    {
                        // output = new string[dialogue.Length + 1];
                        // output[0] = "dialogue";
                        // 
                        // Array.Copy(dialogue, 0, output, 1, dialogue.Length);
                        // screenManager.SetScreen(output);

                        screenManager.SetScreen(ScreenManagerV2.Screen.dialogue, dialogue, true, 1);
                    }

                    state = State.idle;
                    missionStart = false;
                    break;
            }
        }

        if (missionComplete)
        {
            // output = new string[1];
            // output[0] = "HUD";
            // screenManager.SetScreen(output);

            screenManager.SetScreen(ScreenManagerV2.Screen.HUD, null, false, 0);

            // output = new string[2];
            // output[0] = "missionComplete";
            // output[1] = "\nGood work! \n\n" + missionPostString[0];
            // screenManager.SetScreen(output);

            output = new string[1];
            output[0] = "\nGood work! \n\n" + missionPostString[0];
            screenManager.SetScreen(ScreenManagerV2.Screen.missionComplete, output, false, 0);

            gameManager.SetState(GameManager.State.inactive);
            player.SetState(PlayerController.State.controlDisabled);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            currentMission++;

            if (currentMission == numberOfMissions) missionFinal = true;

            missionComplete = false;
        }

        if (missionFailed)
        {
            // output = new string[1];
            // output[0] = "HUD";
            // screenManager.SetScreen(output);

            // output[0] = "clear";
            // screenManager.SetScreen(output);

            screenManager.SetScreen(ScreenManagerV2.Screen.clear, null, false, 0);

            // output = new string[2];
            // output[0] = "missionFailed";
            // output[1] = "\nOh no!\n\nYour tank has been destroyed!\n\nChin up, soldier, we'll get 'em next time!";
            // screenManager.SetScreen(output);

            output = new string[1];
            output[0] = "\nOh no!\n\nYour tank has been destroyed!\n\nChin up, soldier, we'll get 'em next time!";
            screenManager.SetScreen(ScreenManagerV2.Screen.missionFailed, output, false, 0);

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
                    // string[] output = new string[1];
                    // 
                    // output[0] = "black";
                    // screenManager.SetScreen(output);
                    // 
                    // output[0] = "clear";
                    // screenManager.SetScreen(output);

                    screenManager.SetScreen(ScreenManagerV2.Screen.black, null, false, 0);
                    screenManager.SetScreen(ScreenManagerV2.Screen.clear, null, false, 0);

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
                    // output = new string[1];
                    // output[0] = "black";
                    // screenManager.SetScreen(output);

                    screenManager.SetScreen(ScreenManagerV2.Screen.black, null, false, 0);

                    missionNext = false;
                    state = State.idle;
                    StartMission();
                    break;
            }

        }

        if (gameEnd)
        {
            switch (stage)
            {
                case 0:
                    string[] output = new string[1];

                    // output[0] = "black";
                    // screenManager.SetScreen(output);
                    // 
                    // output[0] = "clear";
                    // screenManager.SetScreen(output);

                    screenManager.SetScreen(ScreenManagerV2.Screen.black, null, false, 0);
                    screenManager.SetScreen(ScreenManagerV2.Screen.clear, null, false, 0);

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
                    // output = new string[2];
                    // output[0] = "gameEnd";
                    // output[1] = "You finished the game! Well done!";
                    // screenManager.SetScreen(output);

                    output = new string[1];
                    output[0] = "You finished the game! Well done!";
                    screenManager.SetScreen(ScreenManagerV2.Screen.gameEnd, output, false, 0);

                    // output = new string[1];
                    // output[0] = "black";
                    // screenManager.SetScreen(output);

                    screenManager.SetScreen(ScreenManagerV2.Screen.black, null, false, 0);

                    gameEnd = false;
                    state = State.idle;
                    break;
            }

        }

        if (quit)
        {
            switch (stage)
            {
                case 0:
                    string[] output = new string[1];

                    // output[0] = "black";
                    // screenManager.SetScreen(output);
                    // 
                    // output[0] = "clear";
                    // screenManager.SetScreen(output);

                    screenManager.SetScreen(ScreenManagerV2.Screen.black, null, false, 0);
                    screenManager.SetScreen(ScreenManagerV2.Screen.clear, null, false, 0);

                    stage++;

                    State next = State.advanceMission;
                    missionQueue.Enqueue(next);
                    break;

                case 1:
                    RunReset();
                    stage++;
                    SetMissions();
                    missionFinal = false;
                    currentMission = 0;
                    next = State.advanceMission;
                    missionQueue.Enqueue(next);
                    break;

                case 2:
                    // output = new string[1];
                    // output[0] = "title";
                    // screenManager.SetScreen(output);
                    // 
                    // output[0] = "black";
                    // screenManager.SetScreen(output);

                    screenManager.SetScreen(ScreenManagerV2.Screen.title, null, false, 0);
                    screenManager.SetScreen(ScreenManagerV2.Screen.black, null, false, 0);

                    quit = false;
                    state = State.idle;
                    break;
            }

        }

    }

    public void NextMission()
    {
        State next;
        if (missionFinal) next = State.gameEnd;
        else next = State.missionNext;

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
                // output = new string[dialogue.Length + 2];
                // output[0] = "dialogue";
                // output[dialogue.Length + 1] = left + " to go!";
                // Array.Copy(dialogue, 0, output, 1, dialogue.Length);
                // screenManager.SetScreen(output);

                output = new string[dialogue.Length + 1];
                Array.Copy(dialogue, 0, output, 0, dialogue.Length);
                output[dialogue.Length] = left + " to go!";

                screenManager.SetScreen(ScreenManagerV2.Screen.dialogue, output, false, 0);
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

                if (nextObj.activeSelf == false) nextObj.SetActive(true);
                if (nextObj.name == "Gate")
                {
                    Transform tempTransform = nextObj.transform;
                    objReference = tempTransform.GetComponent<Objective>();
                    objReference.ActivateMe();
                }

                if (nextObj.tag == "ObjectiveGroup")
                {
                    Transform firstInGroup = nextObj.transform.GetChild(0);
                    objReference = firstInGroup.GetComponent<Objective>();
                    string[] preStrings = objReference.GetPreStrings();

                    int count = nextObj.transform.childCount;

                    if (preStrings.Length != 0)
                    {
                        // output = new string[(dialogue.Length + preStrings.Length + 2)];
                        // output[0] = "dialogue";
                        // 
                        // Array.Copy(dialogue, 0, output, 1, dialogue.Length);
                        // output[dialogue.Length + 1] = "This next objective has " + count + " parts!";
                        // Array.Copy(preStrings, 0, output, dialogue.Length + 2, preStrings.Length);
                        // 
                        // screenManager.SetScreen(output);

                        output = new string[(dialogue.Length + preStrings.Length)];

                        Array.Copy(dialogue, 0, output, 0, dialogue.Length);
                        Array.Copy(preStrings, 0, output, dialogue.Length, preStrings.Length);
                        screenManager.SetScreen(ScreenManagerV2.Screen.dialogue, output, false, 0);
                    }
                    //else
                    //{
                    //    output = new string[(dialogue.Length + 2)];
                    //    output[0] = "dialogue";
                    //
                    //    Array.Copy(dialogue, 0, output, 1, dialogue.Length);
                    //
                    //    output[dialogue.Length + 1] = "This next objective has " + count + " parts!";
                    //    screenManager.SetScreen(output);
                    //}
                }
                else
                {
                    objReference = nextObj.GetComponent<Objective>();
                    string[] preStrings = objReference.GetPreStrings();
                    if (preStrings.Length != 0)
                    {
                        output = new string[(dialogue.Length + preStrings.Length)];

                        Array.Copy(dialogue, 0, output, 0, dialogue.Length);
                        Array.Copy(preStrings, 0, output, dialogue.Length, preStrings.Length);
                        screenManager.SetScreen(ScreenManagerV2.Screen.dialogue, output, false, 0);
                    }
                    else if (dialogue.Length != 0)
                    {
                        screenManager.SetScreen(ScreenManagerV2.Screen.dialogue, dialogue, false, 0);
                    }

                    dialogue = new string[0];
                    
                }
            }
        }
        else
        {
            if (dialogue.Length != 0)
            {
                screenManager.SetScreen(ScreenManagerV2.Screen.dialogue, dialogue, false, 0);
            }

            EndMission();
        }
    }

    public void EndMission()
    {
        timer = endMissionDelay;
        State next = State.missionComplete;
        missionQueue.Enqueue(next);
    }

    public void QuitToMenu()
    {
        State next = State.quitToMenu;
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
        State next = State.missionFailed;
        missionQueue.Enqueue(next);
        countDown = true;
    }
}

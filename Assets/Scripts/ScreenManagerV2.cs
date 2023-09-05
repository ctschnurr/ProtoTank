using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ScreenManagerV2 : MonoBehaviour
{
    GameManager gameManager;
    MissionManager missionManager;
    DialogueManager dialogueManager;
    PlayerController player;

    public enum State
    {
        idle,
        fadeIn,
        fadeInBackground,
        screenAppear,
        screenDisappear,
        fadeOutBackground,
        close,
        fadeOut,
        dialogue,
        fadeHeart,
        addHeart
    }

    public enum Screen
    {
        clear,
        title,
        logo,
        pause,
        missionStart,
        missionComplete,
        missionFailed,
        HUD,
        controls,
        dialogue,
        black,
        missionFinal,
        gameEnd,
        quitScreen
    }

    // public Screen inputScreen = Screen.title;
    public Screen saveScreen = Screen.title;
    GameObject screenObject;

    [Header("Troubleshooting")]
    public State state = State.idle;
    public State pauseState = State.idle;
    public bool isPaused = false;
    public bool controlsUp = false;
    public bool quitUp = false;
    public bool noMenu = false;

    [Header("Screens")]
    public GameObject logoScreen;
    public GameObject titleScreen;
    public GameObject pauseScreen;
    public GameObject missionStart;
    public GameObject missionComplete;
    public GameObject missionFailed;
    public GameObject hud;
    public GameObject controlsScreen;
    public GameObject missionFinal;
    public GameObject gameEnd;
    public GameObject quitScreen;

    GameObject shotFrame;
    Vector3 shotFrameRed;
    Vector3 shotFrameBlack;

    TextMeshProUGUI blackAmmo;

    GameObject heartA;
    GameObject heartB;
    GameObject heartC;
    GameObject heartD;
    GameObject heartTarget;
    Color heartTempColor;
    int playerLives;

    GameObject blackScreen;
    Color blackScreenColor;

    GameObject background;
    Color backgroundColor;

    float screenObjectScale;
    float fadeSpeed = 0.8f;
    float shrinkSpeed = 0.75f;

    string[] messageArray;
    string[] dialogue;
    string message;

    Queue<string[][]> screenQueue;
    Queue<string[][]> screenQueue2;

    string prevString;

    public delegate void FadeOutCompleteAction();
    public static event FadeOutCompleteAction OnFadeOutComplete;

    AudioSource tankAmbience;

    [Header("Audio")]
    public AudioClip titleMusic;
    public AudioClip successSound;
    public AudioClip failSound;
    public AudioClip missionStartSound;

    bool countDown = false;
    public float timer = 0;

    GameManager.State gameState;

    // Start is called before the first frame update
    public void Start()
    {
        tankAmbience = GameObject.Find("Player").GetComponent<AudioSource>();
        titleMusic = Resources.Load<AudioClip>("titleMusic");
        successSound = Resources.Load<AudioClip>("success");
        failSound = Resources.Load<AudioClip>("fail");
        missionStartSound = Resources.Load<AudioClip>("missionStart");

        player = GameObject.Find("Player").GetComponent<PlayerController>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        missionManager = GameObject.Find("MissionManager").GetComponent<MissionManager>();
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();

        blackScreen = GameObject.Find("BlackScreen/Panel");
        blackScreenColor = blackScreen.GetComponent<Image>().color;
        blackScreenColor.a = 1f;
        blackScreen.GetComponent<Image>().color = blackScreenColor;

        background = GameObject.Find("ScreenManager/Background");
        backgroundColor = background.GetComponent<Image>().color;
        backgroundColor.a = .8f;
        background.GetComponent<Image>().color = backgroundColor;

        titleScreen = GameObject.Find("TitleScreen");
        titleScreen.SetActive(false);

        fadeSpeed *= Time.deltaTime;
        shrinkSpeed *= Time.deltaTime;

        logoScreen = GameObject.Find("LogoScreen");
        screenObject = logoScreen;

        pauseScreen = GameObject.Find("PauseScreen");
        pauseScreen.SetActive(false);

        missionStart = GameObject.Find("MissionStartScreen");
        missionStart.SetActive(false);

        missionComplete = GameObject.Find("MissionCompleteScreen");
        missionComplete.SetActive(false);

        missionFailed = GameObject.Find("MissionFailedScreen");
        missionFailed.SetActive(false);

        hud = GameObject.Find("Screens/HUD");

        controlsScreen = GameObject.Find("Controls");
        controlsScreen.SetActive(false);

        missionFinal = GameObject.Find("FinalMissionScreen");
        missionFinal.SetActive(false);

        gameEnd = GameObject.Find("GameEndScreen");
        gameEnd.SetActive(false);

        quitScreen = GameObject.Find("QuitScreen");
        quitScreen.SetActive(false);

        shotFrame = GameObject.Find("HUD/ShotFrame");
        shotFrameRed = shotFrame.transform.position;
        shotFrameBlack = shotFrameRed;
        shotFrameBlack.x += 125;

        heartA = GameObject.Find("Screens/HUD/HeartA");
        heartB = GameObject.Find("Screens/HUD/HeartB");
        heartC = GameObject.Find("Screens/HUD/HeartC");
        heartD = GameObject.Find("Screens/HUD/HeartD");
        heartD.SetActive(false);
        heartTarget = heartC;

        blackAmmo = GameObject.Find("Screens/HUD/BlackAmmo").GetComponent<TextMeshProUGUI>();

        heartTempColor = heartTarget.GetComponent<Image>().color;

        hud.SetActive(false);

        screenQueue = new Queue<string[][]>();
        screenQueue2 = new Queue<string[][]>();

        DialogueManager.OnDialogueEnd += DialogueOver;
        PlayerController.OnPlayerDamage += TakeHeart;
        PlayerController.OnPlayerHeal += AddHeart;
        PlayerController.OnToggleGun += SwitchWeapon;
        MissionManager.OnRunReset += ResetHearts;
    }

    // Update is called once per frame
    void Update()
    {
        blackAmmo.text = "x" + player.GetAmmo();

        gameState = gameManager.GetState();

        if (isPaused)
        {
            switch (pauseState)
            {
                case State.fadeIn:
                    if (blackScreen.GetComponent<Image>().color.a > 0)
                    {
                        blackScreenColor = blackScreen.GetComponent<Image>().color;
                        blackScreenColor.a = Mathf.MoveTowards(blackScreenColor.a, 0f, (fadeSpeed / 4));
                        blackScreen.GetComponent<Image>().color = blackScreenColor;
                    }
                    else
                    {
                        blackScreen.SetActive(false);
                        pauseState = State.idle;
                    }
                    break;

                case State.fadeInBackground:
                    if (background.activeSelf == false) background.SetActive(true);
                    if (background.GetComponent<Image>().color.a < 0.8)
                    {
                        backgroundColor = background.GetComponent<Image>().color;
                        backgroundColor.a = Mathf.MoveTowards(backgroundColor.a, 0.8f, fadeSpeed);
                        background.GetComponent<Image>().color = backgroundColor;
                    }
                    else pauseState = State.screenAppear;
                    break;

                case State.screenAppear:
                    if (screenObject.activeSelf == false)
                    {
                        screenObject.GetComponent<CanvasScaler>().scaleFactor = 0.01f;
                        screenObject.SetActive(true);
                    }
                    if (screenObject.GetComponent<CanvasScaler>().scaleFactor < 1)
                    {
                        screenObjectScale = screenObject.GetComponent<CanvasScaler>().scaleFactor;
                        screenObjectScale = Mathf.MoveTowards(screenObjectScale, 1f, shrinkSpeed);
                        screenObject.GetComponent<CanvasScaler>().scaleFactor = screenObjectScale;
                    }
                    else pauseState = State.idle;
                    break;

                case State.screenDisappear:
                    if (screenObject.GetComponent<CanvasScaler>().scaleFactor > 0.1)
                    {
                        screenObjectScale = screenObject.GetComponent<CanvasScaler>().scaleFactor;
                        screenObjectScale = Mathf.MoveTowards(screenObjectScale, 0.01f, shrinkSpeed);
                        screenObject.GetComponent<CanvasScaler>().scaleFactor = screenObjectScale;

                    }
                    else
                    {
                        screenObject.SetActive(false);

                        if (background.activeSelf == true) state = State.fadeOutBackground;
                        else pauseState = State.idle;
                    }
                    break;

                case State.fadeOutBackground:
                    if (background.GetComponent<Image>().color.a > 0.01)
                    {
                        backgroundColor = background.GetComponent<Image>().color;
                        backgroundColor.a = Mathf.MoveTowards(backgroundColor.a, 0f, fadeSpeed);
                        background.GetComponent<Image>().color = backgroundColor;
                    }
                    else
                    {
                        background.SetActive(false);
                        pauseState = State.idle;
                    }
                    break;

                case State.fadeOut:
                    if (blackScreen.activeSelf == false)
                    {
                        blackScreen.SetActive(true);
                        blackScreenColor.a = 0;
                        blackScreen.GetComponent<Image>().color = blackScreenColor;
                    }
                    if (blackScreen.GetComponent<Image>().color.a < 1)
                    {
                        blackScreenColor = blackScreen.GetComponent<Image>().color;
                        blackScreenColor.a = Mathf.MoveTowards(blackScreenColor.a, 1f, fadeSpeed / 2);
                        blackScreen.GetComponent<Image>().color = blackScreenColor;
                    }
                    else
                    {
                        pauseState = State.idle;
                        FadeOutComplete();
                    }
                    break;

                case State.idle:
                    if (screenQueue.Count != 0)
                    {
                        string[][] nextPauseState = screenQueue.Dequeue();
                        ScreenDequeue(nextPauseState);
                    }
                    break;
            }

            if (gameState == GameManager.State.active && pauseState == State.idle) isPaused = false;
        }
        else
        {
            if (countDown)
            {
                if (timer > 0) timer -= Time.deltaTime;
                if (timer < 0)
                {
                    countDown = false;
                }
            }
            else
            {
                switch (state)
                {
                    case State.idle:
                        break;

                    case State.fadeIn:
                        if (blackScreen.GetComponent<Image>().color.a > 0)
                        {
                            blackScreenColor = blackScreen.GetComponent<Image>().color;
                            blackScreenColor.a = Mathf.MoveTowards(blackScreenColor.a, 0f, (fadeSpeed / 4));
                            blackScreen.GetComponent<Image>().color = blackScreenColor;
                        }
                        else
                        {
                            blackScreen.SetActive(false);
                            state = State.idle;
                        }
                        break;

                    case State.fadeInBackground:
                        if (background.activeSelf == false) background.SetActive(true);
                        if (background.GetComponent<Image>().color.a < 0.8)
                        {
                            backgroundColor = background.GetComponent<Image>().color;
                            backgroundColor.a = Mathf.MoveTowards(backgroundColor.a, 0.8f, fadeSpeed);
                            background.GetComponent<Image>().color = backgroundColor;
                        }
                        else state = State.screenAppear;
                        break;

                    case State.screenAppear:
                        if (saveScreen == Screen.missionStart) noMenu = false;
                        if (screenObject.activeSelf == false)
                        {
                            screenObject.GetComponent<CanvasScaler>().scaleFactor = 0.01f;
                            screenObject.SetActive(true);
                        }
                        if (screenObject.GetComponent<CanvasScaler>().scaleFactor < 1)
                        {
                            screenObjectScale = screenObject.GetComponent<CanvasScaler>().scaleFactor;
                            screenObjectScale = Mathf.MoveTowards(screenObjectScale, 1f, shrinkSpeed);
                            screenObject.GetComponent<CanvasScaler>().scaleFactor = screenObjectScale;
                        }
                        else state = State.idle;
                        break;

                    case State.screenDisappear:
                        if (screenObject.GetComponent<CanvasScaler>().scaleFactor > 0.1)
                        {
                            screenObjectScale = screenObject.GetComponent<CanvasScaler>().scaleFactor;
                            screenObjectScale = Mathf.MoveTowards(screenObjectScale, 0.01f, shrinkSpeed);
                            screenObject.GetComponent<CanvasScaler>().scaleFactor = screenObjectScale;

                            float volNum = screenObjectScale;

                            if (screenObject == titleScreen)
                            {
                                if (tankAmbience.isPlaying)
                                {
                                    tankAmbience.volume = volNum;
                                }
                            }
                        }
                        else
                        {
                            screenObject.SetActive(false);

                            if (tankAmbience.isPlaying && screenObject == titleScreen)
                            {
                                tankAmbience.Stop();
                                tankAmbience.volume = 1;
                            }
                            if (background.activeSelf == true) state = State.fadeOutBackground;
                            else state = State.idle;

                            if (saveScreen == Screen.missionStart) noMenu = true;
                        }
                        break;

                    case State.fadeOutBackground:
                        if (background.GetComponent<Image>().color.a > 0.01)
                        {
                            backgroundColor = background.GetComponent<Image>().color;
                            backgroundColor.a = Mathf.MoveTowards(backgroundColor.a, 0f, fadeSpeed);
                            background.GetComponent<Image>().color = backgroundColor;
                        }
                        else
                        {
                            background.SetActive(false);
                            state = State.idle;
                        }
                        break;

                    case State.fadeOut:
                        if (blackScreen.activeSelf == false)
                        {
                            blackScreen.SetActive(true);
                            blackScreenColor.a = 0;
                            blackScreen.GetComponent<Image>().color = blackScreenColor;
                        }
                        if (blackScreen.GetComponent<Image>().color.a < 1)
                        {
                            blackScreenColor = blackScreen.GetComponent<Image>().color;
                            blackScreenColor.a = Mathf.MoveTowards(blackScreenColor.a, 1f, fadeSpeed / 2);
                            blackScreen.GetComponent<Image>().color = blackScreenColor;
                        }
                        else
                        {
                            state = State.idle;
                            FadeOutComplete();
                        }
                        break;

                    case State.fadeHeart:
                        if (heartTarget.GetComponent<Image>().color.a > 0.1)
                        {
                            heartTempColor = heartTarget.GetComponent<Image>().color;
                            heartTempColor.a = Mathf.MoveTowards(heartTempColor.a, 0f, fadeSpeed);
                            heartTarget.GetComponent<Image>().color = heartTempColor;
                        }
                        else
                        {
                            heartTarget.SetActive(false);
                            state = State.idle;
                        }
                        break;

                    case State.addHeart:

                        if (heartTarget.GetComponent<Image>().color.a < 1)
                        {
                            heartTempColor = heartTarget.GetComponent<Image>().color;
                            heartTempColor.a = Mathf.MoveTowards(heartTempColor.a, 1f, fadeSpeed);
                            heartTarget.GetComponent<Image>().color = heartTempColor;
                        }
                        else
                        {
                            state = State.idle;
                        }
                        break;

                    case State.dialogue:
                        if (dialogueManager.GetState() == DialogueManager.State.idle)
                        {
                            dialogueManager.StartDialogue(dialogue);
                        }
                        break;
                }

                if (screenQueue2.Count != 0 && state == State.idle)
                {
                    string[][] nextState = screenQueue2.Dequeue();
                    ScreenDequeue(nextState);
                }
            }

        }
    }

    void DialogueOver()
    {
        if (state == State.dialogue) state = State.idle;
    }

    public State GetState()
    {
        return state;
    }

    public bool GetIsPaused()
    {
        if (noMenu == false) return true;
        else return isPaused;
    }

    public void ScreenDequeue(string[][] input)
    {
        string[] holder = input[0];
        string screenDat = holder[0];
        Screen inputScreen = (Screen)Enum.Parse(typeof(Screen), screenDat);

        holder = input[2];
        screenDat = holder[0];
        bool inputBool = screenDat == "True";

        holder = input[3];
        screenDat = holder[0];
        float inputFloat = float.Parse(screenDat);

        SetScreen(inputScreen, input[1], inputBool, inputFloat);
    }

    public void ScreenEnqueue(Screen inputScreen, string[] inputStrings, bool needTimer, float inputTimer)
    {

        string[][] toQueue = new string[4][];

        string toParse = inputScreen.ToString();
        string[] toInput = new string[1];
        toQueue[0] = new string[] {toParse};

        toQueue[1] = inputStrings;

        toParse = needTimer.ToString();
        toQueue[2] = new string[] {toParse};

        toParse = inputTimer.ToString();
        toQueue[3] = new string[] {toParse};

        if (isPaused) screenQueue.Enqueue(toQueue);
        else screenQueue2.Enqueue(toQueue);
    }

    public void SecondaryEnqueue(Screen inputScreen, string[] inputStrings, bool needTimer, float inputTimer)
    {

        string[][] toQueue = new string[4][];

        string toParse = inputScreen.ToString();
        string[] toInput = new string[1];
        toQueue[0] = new string[] { toParse };

        toQueue[1] = inputStrings;

        toParse = needTimer.ToString();
        toQueue[2] = new string[] { toParse };

        toParse = inputTimer.ToString();
        toQueue[3] = new string[] { toParse };

        screenQueue2.Enqueue(toQueue);
    }

    public void SetScreen(Screen inputScreen, string[] inputStrings, bool needTimer, float inputTimer)
    {
        if (isPaused)
        {
            if (pauseState != State.idle)
            {
                ScreenEnqueue(inputScreen, inputStrings, needTimer, inputTimer);
            }
            else
            {
                switch (inputScreen)
                {
                    case Screen.clear:
                        if (titleScreen.activeSelf == true) screenObject = titleScreen;
                        else if (pauseScreen.activeSelf == true) screenObject = pauseScreen;
                        else if (missionStart.activeSelf == true) screenObject = missionStart;
                        else if (missionComplete.activeSelf == true) screenObject = missionComplete;
                        else if (gameEnd.activeSelf == true) screenObject = gameEnd;
                        else if (controlsScreen.activeSelf == true) screenObject = controlsScreen;
                        else if (quitScreen.activeSelf == true) screenObject = quitScreen;
                        pauseState = State.screenDisappear;
                        return;

                    case Screen.pause:
                        saveScreen = Screen.pause;
                        screenObject = pauseScreen;
                        pauseState = State.screenAppear;
                        break;

                    case Screen.controls:
                        // currentScreen = Screen.pause;
                        screenObject = controlsScreen;
                        pauseState = State.screenAppear;
                        break;

                    case Screen.quitScreen:
                        // currentScreen = Screen.pause;
                        screenObject = quitScreen;
                        pauseState = State.screenAppear;
                        break;

                    case Screen.dialogue:
                        SecondaryEnqueue(inputScreen, inputStrings, needTimer, inputTimer);
                        break;
                }
            }
        }
        else
        {
            if (inputScreen == Screen.pause && noMenu) isPaused = true;

            if (state != State.idle && inputScreen != Screen.pause)
            {
                ScreenEnqueue(inputScreen, inputStrings, needTimer, inputTimer);
            }
            else
            {
                if (needTimer)
                {
                    countDown = true;
                    timer = inputTimer;
                }

                switch (inputScreen)
                {
                    case Screen.clear:
                        if (titleScreen.activeSelf == true) screenObject = titleScreen;
                        else if (pauseScreen.activeSelf == true) screenObject = pauseScreen;
                        else if (missionStart.activeSelf == true) screenObject = missionStart;
                        else if (missionComplete.activeSelf == true) screenObject = missionComplete;
                        else if (gameEnd.activeSelf == true) screenObject = gameEnd;
                        else if (controlsScreen.activeSelf == true) screenObject = controlsScreen;
                        else if (quitScreen.activeSelf == true) screenObject = quitScreen;
                        else if (hud.activeSelf == true) screenObject = hud;
                        state = State.screenDisappear;
                        if (dialogueManager.GetState() != DialogueManager.State.idle)
                        {
                            dialogueManager.ClearDialogue();
                        }
                        return;

                    case Screen.logo:
                        //currentScreen = Screen.logo;
                        screenObject = logoScreen;
                        state = State.fadeInBackground;

                        // if (!tankAmbience.isPlaying)
                        // {
                        //     tankAmbience.clip = titleMusic;
                        //     tankAmbience.loop = true;
                        //     tankAmbience.Play();
                        // }
                        break;

                    case Screen.title:
                        saveScreen = Screen.title;
                        screenObject = titleScreen;
                        state = State.fadeInBackground;
                        break;

                    case Screen.pause:
                        saveScreen = Screen.pause;
                        screenObject = pauseScreen;
                        pauseState = State.screenAppear;
                        break;

                    case Screen.controls:
                        // currentScreen = Screen.pause;
                        screenObject = controlsScreen;
                        pauseState = State.screenAppear;
                        break;

                    case Screen.quitScreen:
                        // currentScreen = Screen.pause;
                        screenObject = quitScreen;
                        pauseState = State.screenAppear;
                        break;

                    case Screen.missionStart:
                        // currentScreen = Screen.pause;
                        screenObject = missionStart;
                        saveScreen = Screen.missionStart;
                        state = State.screenAppear;
                        dialogueManager.InfoBox(inputStrings);

                        tankAmbience.volume = 1;
                        tankAmbience.PlayOneShot(missionStartSound);
                        break;

                    case Screen.missionComplete:
                        // currentScreen = Screen.pause;
                        screenObject = missionComplete;
                        state = State.fadeInBackground;
                        dialogueManager.InfoBox(inputStrings);

                        tankAmbience.PlayOneShot(successSound);
                        break;

                    case Screen.missionFailed:
                        saveScreen = Screen.missionFailed;
                        screenObject = missionFailed;
                        state = State.fadeInBackground;
                        dialogueManager.InfoBox(inputStrings);

                        tankAmbience.PlayOneShot(failSound);
                        break;

                    case Screen.HUD:
                        // currentScreen = Screen.HUD;
                        screenObject = hud;
                        if (hud.activeSelf == false) state = State.screenAppear;
                        else state = State.screenDisappear;
                        break;

                    case Screen.dialogue:
                        // currentScreen = Screen.dialogue;
                        state = State.dialogue;
                        // dialogueManager.StartDialogue(inputStrings);
                        dialogue = new string[inputStrings.Length];
                        dialogue = inputStrings;
                        break;

                    case Screen.black:
                        // currentScreen = Screen.black;
                        if (blackScreen.activeSelf == false) state = State.fadeOut;
                        else state = State.fadeIn;
                        break;

                    case Screen.missionFinal:
                        // currentScreen = Screen.pause;
                        screenObject = missionFinal;
                        state = State.fadeInBackground;
                        dialogueManager.InfoBox(inputStrings);

                        tankAmbience.PlayOneShot(missionStartSound);
                        break;

                    case Screen.gameEnd:
                        saveScreen = Screen.gameEnd;
                        screenObject = gameEnd;
                        state = State.fadeInBackground;
                        dialogueManager.InfoBox(inputStrings);

                        if (tankAmbience.isPlaying) tankAmbience.Stop();
                        tankAmbience.clip = titleMusic;
                        tankAmbience.loop = true;
                        tankAmbience.Play();
                        break;

                    default:
                        Debug.Log("screen didn't parse properly");
                        break;
                }
            }

        }
    }

    public void FadeOutComplete()
    {
        if (OnFadeOutComplete != null)
        {
            OnFadeOutComplete();
        }
    }

    void TakeHeart()
    {
        playerLives = player.GetLives();
        if (playerLives == 1) heartTarget = heartA;
        else if (playerLives == 2) heartTarget = heartB;
        else if (playerLives == 3) heartTarget = heartC;
        else if (playerLives == 4) heartTarget = heartD;

        state = State.fadeHeart;
    }

    void AddHeart()
    {
        playerLives = player.GetLives();
        if (playerLives == 2) heartTarget = heartB;
        else if (playerLives == 3) heartTarget = heartC;
        else if (playerLives == 4) heartTarget = heartD;

        if (heartTarget.activeSelf == false) heartTarget.SetActive(true);

        heartTempColor.a = 0f;
        heartTarget.GetComponent<Image>().color = heartTempColor;

        state = State.addHeart;
    }

    public void ResetHearts()
    {
        heartTempColor.a = 1.0f;
        heartA.GetComponent<Image>().color = heartTempColor;
        heartA.SetActive(true);
        heartB.GetComponent<Image>().color = heartTempColor;
        heartB.SetActive(true);
        heartC.GetComponent<Image>().color = heartTempColor;
        heartC.SetActive(true);
    }

    public void SwitchWeapon()
    {
        if (shotFrame.transform.position == shotFrameRed) shotFrame.transform.position = shotFrameBlack;
        else if (shotFrame.transform.position == shotFrameBlack) shotFrame.transform.position = shotFrameRed;
    }

    public void ToggleControlsScreen()
    {
        string[] output = new string[1];

        if (!controlsUp)
        {
            SetScreen(Screen.clear, null, false, 0);
            SetScreen(Screen.controls, null, false, 0);
            controlsUp = true;
        }

        else if (controlsUp)
        {
            SetScreen(Screen.clear, null, false, 0);
            SetScreen(Screen.pause, null, false, 0);
            controlsUp = false;
        }
    }

    public void ToggleQuitScreen()
    {
        if (!quitUp)
        {
            SetScreen(Screen.clear, null, false, 0);
            SetScreen(Screen.quitScreen, null, false, 0);
            quitUp = true;
        }

        else if (quitUp)
        {
            SetScreen(Screen.clear, null, false, 0);
            SetScreen(saveScreen, null, false, 0);
            quitUp = false;
        }
    }
}

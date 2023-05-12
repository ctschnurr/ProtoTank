using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ScreenManager : MonoBehaviour
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
        pause,
        missionStart,
        missionComplete,
        missionFailed,
        HUD,
        dialogue,
        black
    }

    public State state = State.fadeIn;
    public Screen inputScreen = Screen.title;
    public Screen currentScreen = Screen.title;
    GameObject screenObject;

    GameObject titleScreen;
    GameObject pauseScreen;
    GameObject missionStart;
    GameObject missionComplete;
    GameObject missionFailed;
    GameObject hud;

    GameObject shotFrame;
    Vector3 shotFrameRed;
    Vector3 shotFrameBlack;

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
    float shrinkSpeed = 0.80f;

    string[] messageArray;
    string message;

    Queue<string[]> screenQueue;

    int lifetracker = 3;

    public delegate void FadeOutCompleteAction();
    public static event FadeOutCompleteAction OnFadeOutComplete;

    // Start is called before the first frame update
    public void Start()
    {
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
        screenObject = titleScreen;
        fadeSpeed *= Time.deltaTime;
        shrinkSpeed *= Time.deltaTime;

        pauseScreen = GameObject.Find("PauseScreen");
        pauseScreen.SetActive(false);

        missionStart = GameObject.Find("MissionStartScreen");
        missionStart.SetActive(false);

        missionComplete = GameObject.Find("MissionCompleteScreen");
        missionComplete.SetActive(false);

        missionFailed = GameObject.Find("MissionFailedScreen");
        missionFailed.SetActive(false);

        hud = GameObject.Find("HUD");

        shotFrame = GameObject.Find("HUD/ShotFrame");
        shotFrameRed = shotFrame.transform.position;
        shotFrameBlack = shotFrameRed;
        shotFrameBlack.x += 125;

        heartA = GameObject.Find("HUD/HeartA");
        heartB = GameObject.Find("HUD/HeartB");
        heartC = GameObject.Find("HUD/HeartC");
        heartD = GameObject.Find("HUD/HeartD");
        heartD.SetActive(false);
        heartTarget = heartC;

        heartTempColor = heartTarget.GetComponent<Image>().color;

        hud.SetActive(false);

        screenQueue = new Queue<string[]>();

        DialogueManager.OnDialogueEnd += DialogueOver;
        PlayerController.OnPlayerDamage += TakeHeart;
        PlayerController.OnPlayerHeal += AddHeart;
        PlayerController.OnToggleGun += SwitchWeapon;
        MissionManager.OnRunReset += ResetHearts;
    }

    // Update is called once per frame
    void Update()
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
                }
                else
                {
                    screenObject.SetActive(false);
                    if (background.activeSelf == true) state = State.fadeOutBackground;
                    else state = State.idle;
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
        }

        if (screenQueue.Count != 0 && state == State.idle)
        {
            string[] next = screenQueue.Dequeue();
            SetScreen(next);
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

    public void SetScreen(string[] input)
    {
        if (state != State.idle) screenQueue.Enqueue(input);
        else
        {
            string screenDat = input[0];
            inputScreen = (Screen)Enum.Parse(typeof(Screen), screenDat);

            if (input.Length == 2)
            {
                messageArray = new string[1];
                messageArray[0] = input[1];
            }
            else if (input.Length > 2)
            {
                messageArray = new string[input.Length - 1];
                Array.Copy(input, 1, messageArray, 0, (input.Length - 1));
            }

            switch (inputScreen)
            {
                case Screen.clear:
                    if (titleScreen.activeSelf == true) screenObject = titleScreen;
                    else if(pauseScreen.activeSelf == true) screenObject = pauseScreen;
                    else if(missionStart.activeSelf == true) screenObject = missionStart;
                    else if(missionComplete.activeSelf == true) screenObject = missionComplete;
                    else if(hud.activeSelf == true) screenObject = hud;
                    state = State.screenDisappear;
                    if (dialogueManager.GetState() != DialogueManager.State.idle)
                    {
                        dialogueManager.ClearDialogue();
                    }
                    return;

                case Screen.title:
                    currentScreen = Screen.pause;
                    screenObject = titleScreen;
                    state = State.fadeInBackground;
                    break;

                case Screen.pause:
                    currentScreen = Screen.pause;
                    screenObject = pauseScreen;
                    state = State.fadeInBackground;
                    break;

                case Screen.missionStart:
                    currentScreen = Screen.pause;
                    screenObject = missionStart;
                    state = State.screenAppear;
                    dialogueManager.InfoBox(messageArray);
                    break;

                case Screen.missionComplete:
                    currentScreen = Screen.pause;
                    screenObject = missionComplete;
                    state = State.fadeInBackground;
                    dialogueManager.InfoBox(messageArray);
                    break;

                case Screen.missionFailed:
                    currentScreen = Screen.pause;
                    screenObject = missionFailed;
                    state = State.fadeInBackground;
                    dialogueManager.InfoBox(messageArray);
                    break;

                case Screen.HUD:
                    currentScreen = Screen.HUD;
                    screenObject = hud;
                    if (hud.activeSelf == false) state = State.screenAppear;
                    else state = State.screenDisappear;
                    break;

                case Screen.dialogue:
                    state = State.dialogue;
                    currentScreen = Screen.dialogue;
                    dialogueManager.StartDialogue(messageArray);
                    break;

                case Screen.black:
                    currentScreen = Screen.black;
                    if (blackScreen.activeSelf == false) state = State.fadeOut;
                    else state = State.fadeIn;
                    break;

                default:
                    Debug.Log("screen didn't parse properly");
                    break;
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
}

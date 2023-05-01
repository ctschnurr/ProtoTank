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

    public enum State
    {
        idle,
        fadeIn,
        fadeInBackground,
        screenAppear,
        screenDisappear,
        fadeOutBackground,
        close,
        fadeOut
    }

    public enum Screen
    {
        clear,
        title,
        pause,
        missionStart,
        missionComplete,
        HUD
    }

    public State state = State.fadeIn;
    public Screen inputScreen = Screen.title;
    public Screen currentScreen = Screen.title;
    GameObject screenObject;

    GameObject titleScreen;
    GameObject pauseScreen;
    GameObject missionStart;
    GameObject missionComplete;
    GameObject hud;

    GameObject blackScreen;
    Color blackScreenColor;

    GameObject background;
    Color backgroundColor;

    float screenObjectScale;
    float fadeSpeed = 0.8f;
    float shrinkSpeed = 0.80f;

    bool dialogueClear = true;
    string[] messageArray;
    string message;

    Queue<string[]> screenQueue;

    // Start is called before the first frame update
    public void Start()
    {
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

        hud = GameObject.Find("HUD");
        hud.SetActive(false);

        screenQueue = new Queue<string[]>();
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
                }
                break;
        }

        if (screenQueue.Count != 0 && state == State.idle)
        {
            string[] next = screenQueue.Dequeue();
            SetScreen(next);
        }
    }

    void CheckQueue()
    {

    }

    public State GetState()
    {
        return state;
    }

    //public void SetScreen(Screen input, string message)
    public void SetScreen(string[] input)
    {
        if (state != State.idle) screenQueue.Enqueue(input);
        else
        {
            string screenDat = input[0];
            inputScreen = (Screen)Enum.Parse(typeof(Screen), screenDat);

            if (input.Length == 2)
            {
                message = input[1];
            }
            else if (input.Length > 2)
            {
                messageArray = new string[input.Length - 1];
                Array.Copy(input, 1, messageArray, 0, (input.Length - 1));
            }

            switch (inputScreen)
            {
                case Screen.clear:
                    state = State.screenDisappear;
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
                    dialogueManager.InfoBox(message);
                    break;

                case Screen.missionComplete:
                    currentScreen = Screen.pause;
                    screenObject = missionComplete;
                    state = State.screenAppear;
                    break;

                case Screen.HUD:
                    currentScreen = Screen.HUD;
                    screenObject = hud;
                    state = State.screenAppear;
                    break;

                default:
                    Debug.Log("screen didn't parse properly");
                    break;
            }


        }

    }
}

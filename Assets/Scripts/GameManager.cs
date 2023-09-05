using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum State
    {
        inactive,
        active,
        paused,
        dead
    }

    ScreenManagerV2 screenManager;
    PlayerController player;
    MissionManager missionManager;

    DialogueManager dialogueManager;

    public State state = State.inactive;

    // Start is called before the first frame update
    void Start()
    {
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManagerV2>();
        missionManager = GameObject.Find("MissionManager").GetComponent<MissionManager>();
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();

        player = GameObject.Find("Player").GetComponent<PlayerController>();
        player.SetState(PlayerController.State.controlDisabled);

        // DialogueManager.OnDialogueEnd += ReceiveEvent; // like saying 'start listening'
        // DialogueManager.OnDialogueEnd -= ReceiveEvent; // like saying 'stop listening'

        screenManager.SetScreen(ScreenManagerV2.Screen.black, null, true, 2);
        screenManager.SetScreen(ScreenManagerV2.Screen.black, null, true, 2);
        screenManager.SetScreen(ScreenManagerV2.Screen.clear, null, false, 0);
        screenManager.SetScreen(ScreenManagerV2.Screen.title, null, false, 0);
        screenManager.SetScreen(ScreenManagerV2.Screen.black, null, false, 1);
    }

    // Update is called once per frame
    void Update()
    {
        string[] output = new string[1];
        switch (state)
        {
            case State.inactive:

                break;

            case State.active:

                break;

            case State.paused:

                break;

            case State.dead:

                break;
        }
    }

    static void ReceiveEvent()
    {
        Debug.Log("Event Received!");
    }

    public void StartGame()
    {
        // string[] output = new string[1];
        // output[0] = "clear";
        screenManager.SetScreen(ScreenManagerV2.Screen.clear, null, false, 0);
        MissionManager.StartMission();
    }

    public void PauseGame()
    {
        SetState(State.paused);
    }

    public void UnPauseGame()
    {
        SetState(State.active);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public State GetState()
    {
        return state;
    }

    public void SetState(State input)
    {
        string[] output = new string[1];

        switch (input)
        {
            case State.active:

                // if (screenManager.GetIsPaused() == true)
                // {
                //     Time.timeScale = 1;
                //     player.SetState(PlayerController.State.controlEnabled);
                //     // output = new string[1];
                //     // output[0] = "clear";
                //     screenManager.SetScreen(ScreenManagerV2.Screen.clear, null, false, 0);
                // 
                //     Cursor.lockState = CursorLockMode.Locked;
                //     Cursor.visible = false;
                // }

                Time.timeScale = 1;
                player.SetState(PlayerController.State.controlEnabled);
                // output = new string[1];
                // output[0] = "clear";
                screenManager.SetScreen(ScreenManagerV2.Screen.clear, null, false, 0);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                //

                if (dialogueManager.GetState() != DialogueManager.State.idle)
                {
                    dialogueManager.ClearDialogue();
                }

                state = State.active;
                break;

            case State.dead:
                // output = new string[2];
                // output[0] = "missionFailed";
                // output[1] = "I'm afraid you are dead!";
                output = new string[1];
                output[0] = "I'm afraid you are dead!";
                screenManager.SetScreen(ScreenManagerV2.Screen.missionFailed, output, false, 0);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                state = State.dead;
                break;

            case State.paused:
                if (screenManager.GetIsPaused() == false)
                {
                    Time.timeScale = 0;
                    player.SetState(PlayerController.State.controlDisabled);
                    // output = new string[1];
                    // output[0] = "pause";
                    screenManager.SetScreen(ScreenManagerV2.Screen.pause, null, false, 0);
                
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                
                    state = State.paused;
                }
                break;

            case State.inactive:
                state = State.inactive;
                break;


        }
    }

}

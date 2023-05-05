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

    ScreenManager screenManager;
    PlayerController player;
    MissionManager missionManager;

    DialogueManager dialogueManager;

    public State state = State.inactive;

    // Start is called before the first frame update
    void Start()
    {
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();
        missionManager = GameObject.Find("MissionManager").GetComponent<MissionManager>();
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();

        player = GameObject.Find("Player").GetComponent<PlayerController>();
        player.SetState(PlayerController.State.controlDisabled);

        // DialogueManager.OnDialogueEnd += ReceiveEvent; // like saying 'start listening'
        // DialogueManager.OnDialogueEnd -= ReceiveEvent; // like saying 'stop listening'
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
                if (screenManager.GetState() == ScreenManager.State.idle)
                {
                    Time.timeScale = 0;
                    player.SetState(PlayerController.State.controlDisabled);
                    output = new string[1];
                    output[0] = "pause";
                    screenManager.SetScreen(output);

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
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
        string[] output = new string[1];
        output[0] = "clear";
        screenManager.SetScreen(output);
        MissionManager.StartMission();
    }

    public void PauseGame()
    {
        state = State.paused;
    }

    public void UnPauseGame()
    {
        state = State.active;
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

                if (screenManager.GetState() == ScreenManager.State.idle)
                {
                    Time.timeScale = 1;
                    player.SetState(PlayerController.State.controlEnabled);
                    output = new string[1];
                    output[0] = "clear";
                    screenManager.SetScreen(output);

                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }

                if (dialogueManager.GetState() != DialogueManager.State.idle)
                {
                    dialogueManager.ClearDialogue();
                }

                state = State.active;
                break;

            case State.dead:
                output = new string[2];
                output[0] = "missionFailed";
                output[1] = "I'm afraid you are dead!";
                screenManager.SetScreen(output);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                state = State.dead;
                break;


        }
    }

}

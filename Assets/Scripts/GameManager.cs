using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    ScreenManager screenManager;
    PlayerController player;
    MissionManager missionManager;

    DialogueManager dialogueManager;

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
        if (screenManager.GetState() == ScreenManager.State.idle)
        {
            Time.timeScale = 0;
            player.SetState(PlayerController.State.controlDisabled);
            string[] output = new string[1];
            output[0] = "pause";
            screenManager.SetScreen(output);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UnPauseGame()
    {
        if (screenManager.GetState() == ScreenManager.State.idle)
        {
            Time.timeScale = 1;
            player.SetState(PlayerController.State.controlEnabled);
            string[] output = new string[1];
            output[0] = "clear";
            screenManager.SetScreen(output);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (dialogueManager.GetState() != DialogueManager.State.idle)
        {
            dialogueManager.ClearDialogue();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

}

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        screenManager.SetScreen(ScreenManager.Screen.clear);
        missionManager.StartMission();
    }

    public void PauseGame()
    {
        if (screenManager.GetState() == ScreenManager.State.idle)
        {
            Time.timeScale = 0;
            player.SetState(PlayerController.State.controlDisabled);
            screenManager.SetScreen(ScreenManager.Screen.pause);

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
            screenManager.SetScreen(ScreenManager.Screen.clear);

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

    public void NextMission()
    {

    }
}

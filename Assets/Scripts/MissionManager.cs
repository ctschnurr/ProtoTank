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

    GameObject parent;
    GameObject nextCheckPoint;
    GameObject reference;

    GameObject missionFolder1;
    int numberOfMissions;

    public enum Mission
    {
        mission1,
        mission2,
        mission3
    }

    static int currentMission = 0;

    static int stage = 0;
    static float timer = 2;

    static string[] output;
    static bool countDown = false;

    List<GameObject>[] missions;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();

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
            }
        }

        List<GameObject> holder = missions[currentMission];

        for (int i = 1; i < missions[currentMission].Count; i++)
        {
            GameObject deactivateMe = holder[i];
            deactivateMe.SetActive(false);
        }
        //-----

        DialogueManager.OnDialogueEnd += NextStage;

    }

    // Update is called once per frame
    void Update()
    {
        if (countDown)
        {
            if (timer > 0) timer -= Time.deltaTime;
            if (timer < 0)
            {
                AdvanceMission();
                countDown = false;
            }
        }
    }

    public static void AdvanceMission()
    {
        switch (stage)
        {
            case 0:
                output = new string[2];
                output[0] = "missionStart";
                output[1] = "For your first mission, we will simply run through some exercises!";
                screenManager.SetScreen(output);
                timer = 1;
                break;

            case 1:
                output = new string[1];
                output[0] = "HUD";
                screenManager.SetScreen(output);
                timer = 5;
                break;

            case 99:
                    output = new string[2];
                    output[0] = "missionComplete";
                    output[1] = "Click CONTINUE to move on to the next mission.";
                    screenManager.SetScreen(output);

                    // Time.timeScale = 0;
                    player.SetState(PlayerController.State.controlDisabled);

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    currentMission++;
                break;
        }
    }

    public GameObject GetNextCheckpoint()
    {
        List<GameObject> holder = missions[currentMission];
        return holder[0];
    }

    public void NextObjective(GameObject input, string[] dialogue)
    {
        reference = input;
        //reference.SetActive(false);
        missions[currentMission].Remove(reference);

        output = new string[dialogue.Length + 1];
        output[0] = "dialogue";

        Array.Copy(dialogue, 0, output, 1, dialogue.Length);

        screenManager.SetScreen(output);

        if (missions[currentMission].Count != 0)
        {
            List<GameObject> refList = missions[currentMission];
            refList[0].SetActive(true);
        }
        else
        {
            stage = 99;
            countDown = true;
        }
    }

    static void NextStage()
    {
        if (stage != 99) stage++;
        countDown = true;
    }
}

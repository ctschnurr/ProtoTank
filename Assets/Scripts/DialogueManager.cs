using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// public delegate void DialogueEventHandler();

public class DialogueManager : MonoBehaviour
{
    GameObject dialogueWindow;
    Color windowColor;

    TextMeshProUGUI dialogueText;
    GameObject dialogueTextObject;
    Color textColor;

    public enum State
    {
        idle,
        fadeIn,
        open,
        textIn,
        textOut,
        close,
        fadeOut
    }

    public State state = State.idle;

    float fadeSpeed = 0.5f;
    float sizeSpeed = 500f;

    Vector2 sizeReference;
    Vector2 maxSize = new Vector2(800, 200);
    Vector2 infoSize = new Vector2(800, 200);
    Vector2 minSize = new Vector2(50, 50);

    Vector2 infoPosition = new Vector2(0, 400);
    Vector2 dialoguePosition = new Vector2(0, 50);


    Queue<string> dialogue;

    bool running = false;

    float timer = 3f;
    float timerReset = 3f;

    bool timed = false;
    bool skip = false;

    public delegate void DialogueEndAction();
    public static event DialogueEndAction OnDialogueEnd;

    // Start is called before the first frame update
    void Start()
    {
        dialogueWindow = GameObject.Find("DialogueManager/DialogueWindow");
        windowColor = dialogueWindow.GetComponent<Image>().color;
        windowColor.a = 0f;
        dialogueWindow.GetComponent<Image>().color = windowColor;

        dialogueTextObject = GameObject.Find("DialogueManager/DialogueText");
        dialogueText = GameObject.Find("DialogueManager/DialogueText").GetComponent<TextMeshProUGUI>();
        textColor = dialogueText.color;
        textColor.a = 0f;
        dialogueText.color = textColor;

        fadeSpeed *= Time.deltaTime;
        sizeSpeed *= Time.deltaTime;

        dialogue = new Queue<string>();

        string[] test = new string[2];

        test[0] = "This is a test message!";
        test[1] = "Please disregard!";

        // timer *= Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.idle:
                break;

            case State.fadeIn:
                if (dialogueWindow.GetComponent<Image>().color.a < 0.6)
                {
                    windowColor = dialogueWindow.GetComponent<Image>().color;
                    windowColor.a = Mathf.MoveTowards(windowColor.a, 0.6f, fadeSpeed);
                    dialogueWindow.GetComponent<Image>().color = windowColor;
                }
                else state = State.open;
                break;

            case State.open:
                Vector2 tempSize = dialogueWindow.GetComponent<RectTransform>().sizeDelta;

                if (tempSize.x <= sizeReference.x)
                {
                    tempSize.x = tempSize.x + sizeSpeed;
                    dialogueWindow.GetComponent<RectTransform>().sizeDelta = tempSize;
                }

                else if (tempSize.y <= sizeReference.y)
                {
                    tempSize.y = tempSize.y + (sizeSpeed * .5f);
                    dialogueWindow.GetComponent<RectTransform>().sizeDelta = tempSize;
                }

                if (tempSize.y >= sizeReference.y) state = State.textIn;

                break;

            case State.textIn:
                if (dialogueText.color.a < 0.99)
                {
                    textColor = dialogueText.color;
                    textColor.a = Mathf.MoveTowards(textColor.a, 1f, fadeSpeed);
                    dialogueText.color = textColor;
                }
                else
                {
                    switch (timed)
                    {
                        case false:
                            break;

                        case true:
                            if (skip)
                            {
                                timer = timerReset;
                                NextPage();
                                skip = false;
                            }
                            else
                            {
                                if (timer > 0) timer -= Time.deltaTime;
                                if (timer < 0) NextPage();
                            }
                            break;
                    }
                }
                break;

            case State.textOut:
                if (dialogueText.color.a > 0.01)
                {
                    textColor = dialogueText.color;
                    textColor.a = Mathf.MoveTowards(textColor.a, 0f, fadeSpeed);
                    dialogueText.color = textColor;
                }
                else state = State.close;
                break;

            case State.close:
                tempSize = dialogueWindow.GetComponent<RectTransform>().sizeDelta;

                if (tempSize.y >= minSize.y)
                {
                    tempSize.y = tempSize.y - (sizeSpeed * .5f);
                    dialogueWindow.GetComponent<RectTransform>().sizeDelta = tempSize;
                }

                else if (tempSize.x >= minSize.x)
                {
                    tempSize.x = tempSize.x - sizeSpeed;
                    dialogueWindow.GetComponent<RectTransform>().sizeDelta = tempSize;
                }

                if (tempSize.x <= minSize.x) state = State.fadeOut;
                break;

            case State.fadeOut:
                if (dialogueWindow.GetComponent<Image>().color.a > 0)
                {
                    windowColor = dialogueWindow.GetComponent<Image>().color;
                    windowColor.a = Mathf.MoveTowards(windowColor.a, 0f, fadeSpeed);
                    dialogueWindow.GetComponent<Image>().color = windowColor;
                }
                else
                {
                    DialogueEnd();
                    state = State.idle;
                }
                running = false;
                break;
        }
    }

    public void StartDialogue(string[] input)
    {
        if (!running)
        {
            dialogueWindow.GetComponent<RectTransform>().anchoredPosition = dialoguePosition;
            dialogueTextObject.GetComponent<RectTransform>().anchoredPosition = dialoguePosition;

            sizeReference = maxSize;
            timed = true;
            running = true;
            dialogue.Clear();

            foreach (string line in input)
            {
                dialogue.Enqueue(line);
            }

            string load1st = dialogue.Dequeue();
            dialogueText.text = load1st;
            state = State.fadeIn;
        }
    }

    public void InfoBox(string[] input)
    {
        if (!running)
        {
            dialogueWindow.GetComponent<RectTransform>().anchoredPosition = infoPosition;
            dialogueTextObject.GetComponent<RectTransform>().anchoredPosition = infoPosition;
            sizeReference = infoSize;
            timed = false;
            running = true;

            dialogueText.text = input[0];
            state = State.fadeIn;
        }
    }

    public void NextPage()
    {
        if(dialogue.Count == 0)
        {
            state = State.textOut;
            timer = timerReset;
            return;
        }

        string line = dialogue.Dequeue();
        dialogueText.text = line;
        timer = timerReset;
    }

    public void ClearDialogue()
    {
        state = State.textOut;
    }

    public State GetState()
    {
        return state;
    }

    public void DialogueEnd()
    {
        if (OnDialogueEnd != null)
        {
            OnDialogueEnd();
        }
    }

    public void SetSkip()
    {
        if (state == State.textIn)
        {
            skip = true;
        }
    }

}

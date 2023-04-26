using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    GameObject dialogueWindow;
    Color windowColor;

    TextMeshProUGUI dialogueText;
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
    float sizeSpeed = 400f;

    public Vector2 maxSize = new Vector2(1000, 300);
    Vector2 minSize = new Vector2(50, 50);

    Queue<string> dialogue;

    bool running = false;

    // Start is called before the first frame update
    void Start()
    {
        dialogueWindow = GameObject.Find("DialogueScreen/DialogueWindow");
        windowColor = dialogueWindow.GetComponent<Image>().color;
        windowColor.a = 0f;
        dialogueWindow.GetComponent<Image>().color = windowColor;

        dialogueText = GameObject.Find("DialogueScreen/DialogueText").GetComponent<TextMeshProUGUI>();
        textColor = dialogueText.color;
        textColor.a = 0f;
        dialogueText.color = textColor;

        fadeSpeed *= Time.deltaTime;
        sizeSpeed *= Time.deltaTime;

        dialogue = new Queue<string>();

        string[] test = new string[2];

        test[0] = "This is a test message!";
        test[1] = "Please disregard!";

        // StartDialogue(test);
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

                if (tempSize.x <= maxSize.x)
                {
                    tempSize.x = tempSize.x + sizeSpeed;
                    dialogueWindow.GetComponent<RectTransform>().sizeDelta = tempSize;
                }

                else if (tempSize.y <= maxSize.y)
                {
                    tempSize.y = tempSize.y + (sizeSpeed * .5f);
                    dialogueWindow.GetComponent<RectTransform>().sizeDelta = tempSize;
                }

                if (tempSize.y >= maxSize.y) state = State.textIn;

                break;

            case State.textIn:
                if (dialogueText.color.a < 0.99)
                {
                    textColor = dialogueText.color;
                    textColor.a = Mathf.MoveTowards(textColor.a, 1f, fadeSpeed);
                    dialogueText.color = textColor;
                }

                // - build in mechanic so dialogue process continues until audio clip is done, or player presses skip

                // else state = State.textOut;
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
                else state = State.idle;
                running = false;
                break;
        }
    }

    public void StartDialogue(string[] input)
    {
        if (!running)
        {
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

    public void NextPage()
    {
        if(dialogue.Count == 0)
        {
            state = State.textOut;
            return;
        }

        string line = dialogue.Dequeue();
        dialogueText.text = line;
    }

}

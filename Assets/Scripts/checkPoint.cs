using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkPoint : MonoBehaviour
{
    PlayerController player;
    GameObject checkPointObject;
    GameObject parent;
    MissionManager manager;

    Vector3 respawnPoint;

    Vector3 sizeUp;
    Vector3 sizeDown;

    Color lerpedColor = Color.white;

    Vector3 scaleChange;
    float speed = 0.0001f;
    bool expand = true;
    public bool fadeOut = false;

    Color tempcolor;
    Color savecolor;

    AudioSource checkpointSound;

    public enum State
    {
        Indy,
        Managed
    }

    public State state = State.Indy;

    public string[] dialogueStrings;

    void Start()
    {
        manager = GameObject.Find("MissionManager").GetComponent<MissionManager>();

        player = GameObject.Find("Player").GetComponent<PlayerController>();

        checkPointObject = transform.gameObject;
        respawnPoint = transform.position;

        speed += Time.deltaTime;
        scaleChange = new Vector3(speed, speed, speed);
        tempcolor = GetComponent<Renderer>().material.color;
        savecolor = tempcolor;

        checkpointSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localScale.x > 2f) expand = false; 
        if (transform.localScale.x < 1.75f) expand = true;

        if (expand) transform.localScale += scaleChange;
        if (!expand) transform.localScale -= scaleChange;

        if (fadeOut)
        {
            tempcolor = GetComponent<Renderer>().material.color;
            tempcolor.a = Mathf.MoveTowards(tempcolor.a, 0f, 0.005f);
            GetComponent<Renderer>().material.color = tempcolor;
        }

        if (GetComponent<Renderer>().material.color.a == 0f)
        {
            fadeOut = false;
            GetComponent<Renderer>().material.color = savecolor;
            manager.NextObjective(checkPointObject, dialogueStrings);
            checkPointObject.SetActive(false);
        }

    }

    void TriggerFadeOut()
    {
        fadeOut = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            checkpointSound.Play();

            PlayerController.SetRespawn(respawnPoint);

            //player.SetMessage("Checkpoint!");
            //player.ShowText();

            Invoke("TriggerFadeOut", 0.4f);
        }
    }
}

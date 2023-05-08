using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DroneController : MonoBehaviour
{
    GameManager gameManager;
    MissionManager manager;
    ScreenManager screenManager;

    public enum State
    {
        patrolling,
        dead
    }

    public State state = State.patrolling;
    public UnityEngine.AI.NavMeshAgent enemyDrone;

    GameObject parent;
    GameObject body;

    Transform nextWaypoint;
    Transform waypoint1;
    Transform waypoint2;
    Transform waypoint3;
    Transform waypoint4;
    Transform waypoint5;

    public List<Transform> waypointList;
    public List<GameObject> pieces;

    bool enemyActive = false;
    bool up = false;

    Vector3 posChange;
    float speed = 0.1f;
    Vector3 scaleChange;

    public string[] dialogueStrings;

    bool managed = false;
    bool hasStrings = true;

    float decay_timer = 0.0f;
    float fadeSpeed = 0.1f;

    float alpha = 1f;

    public Component[] droneParts;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        manager = GameObject.Find("MissionManager").GetComponent<MissionManager>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();

        enemyDrone = GetComponent<UnityEngine.AI.NavMeshAgent>();

        parent = transform.gameObject;
        body = parent.transform.Find("Body").gameObject;

        waypoint1 = GameObject.Find("waypoint1").transform;
        waypoint2 = GameObject.Find("waypoint2").transform;
        waypoint3 = GameObject.Find("waypoint3").transform;
        waypoint4 = GameObject.Find("waypoint4").transform;
        waypoint5 = GameObject.Find("waypoint5").transform;

        nextWaypoint = waypoint1;

        waypointList.Add(waypoint1);
        waypointList.Add(waypoint2);
        waypointList.Add(waypoint3);
        waypointList.Add(waypoint4);
        waypointList.Add(waypoint5);

        speed *= Time.deltaTime;
        posChange = new Vector3(0, speed, 0);
        scaleChange = new Vector3(speed * 2, speed * 2, speed * 2);

        decay_timer *= Time.deltaTime;
        fadeSpeed *= Time.deltaTime;

        int numberOfPieces = body.transform.childCount;
        pieces = new List<GameObject>();

        for (int i = 0; i < numberOfPieces; i++)
        {
            Transform dronePieceTransform = body.transform.GetChild(i);
            GameObject dronePiece = dronePieceTransform.gameObject;

            pieces.Add(dronePiece);


        }

        if (transform.parent != null && transform.parent.gameObject.tag == "MissionGroup") managed = true;
        if (dialogueStrings.Length == 0) hasStrings = false;


    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.state == GameManager.State.active) enemyActive = true;
        else enemyActive = false;

        if (enemyActive)
        {
            switch (state)
            {
                case State.patrolling:
                    enemyDrone.isStopped = false;
                    enemyDrone.SetDestination(nextWaypoint.position);

                    if (body.transform.localPosition.y > 1.25f) up = false;
                    if (body.transform.localPosition.y < 1f) up = true;

                    if (up) body.transform.localPosition += posChange;
                    if (!up) body.transform.localPosition -= posChange;

                    if (Vector3.Distance(nextWaypoint.position, transform.position) < 1) nextWaypoint = NextWaypoint(nextWaypoint);
                    break;

                case State.dead:
                    enemyDrone.isStopped = true;
                    decay_timer += 0.05f;

                    if (decay_timer > 10)
                    {
                        droneParts = GetComponentsInChildren<Transform>();

                        foreach (Transform partTransform in droneParts)
                        {
                            partTransform.localScale -= scaleChange;
                            if (partTransform.localScale.x < 0.75) Destroy(partTransform.gameObject);
                        }
                    }
                    break;
            }
        }
        else if (enemyActive == false)
        {
            enemyDrone.isStopped = true;
        }
    }

    Transform NextWaypoint(Transform input)
    {
        // waypointList.Remove(input);
        // waypointList.Add(input);

        int waypointRandom = UnityEngine.Random.Range(1, waypointList.Count);

        return waypointList[waypointRandom];
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Shell")
        {
            state = State.dead;

            foreach (GameObject piece in pieces)
            {
                piece.GetComponent<Rigidbody>().isKinematic = false;
            }

            if (managed) manager.NextObjective(parent, dialogueStrings);
            else if (hasStrings)
            {
                string[] output = new string[dialogueStrings.Length + 1];
                output[0] = "dialogue";

                Array.Copy(dialogueStrings, 0, output, 1, dialogueStrings.Length);
                screenManager.SetScreen(output);
            }
        }
    }
}

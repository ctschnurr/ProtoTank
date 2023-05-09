using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DroneController : Objective
{
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

    float decay_timer = 5.0f;
    float fadeSpeed = 0.1f;

    public Component[] droneParts;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        missionManager = GameObject.Find("MissionManager").GetComponent<MissionManager>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();

        enemyDrone = GetComponent<UnityEngine.AI.NavMeshAgent>();

        parent = transform.gameObject;
        subjectObject = parent;
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

        fadeSpeed *= Time.deltaTime;

        int numberOfPieces = body.transform.childCount;
        pieces = new List<GameObject>();

        for (int i = 0; i < numberOfPieces; i++)
        {
            Transform dronePieceTransform = body.transform.GetChild(i);
            GameObject dronePiece = dronePieceTransform.gameObject;

            pieces.Add(dronePiece);
        }

        if (transform.parent != null)
        {
            if (transform.parent.gameObject.tag == "MissionGroup" || transform.parent.gameObject.tag == "ObjectiveGroup") managed = true;
        }

        tpShader = Shader.Find("Transparent/Diffuse");
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
                    decay_timer -= Time.deltaTime;

                    if (decay_timer < 0)
                    {
                        droneParts = GetComponentsInChildren<Renderer>();

                        foreach (Renderer partRenderer in droneParts)
                        {
                            partRenderer.material.shader = tpShader;

                            Color tempcolor = partRenderer.material.color;
                            tempcolor.a = Mathf.MoveTowards(tempcolor.a, 0f, 0.005f);
                            partRenderer.material.color = tempcolor;

                            if(tempcolor.a == 0)
                            {
                                Destroy(parent);
                            }
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
            RunComplete();

            parent.GetComponent<Collider>().enabled = false;
            foreach (GameObject piece in pieces)
            {
                piece.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }
}
